using System.Diagnostics;
using ChessEngine.Bot;
using Newtonsoft.Json;
using Utility;

// This class will communicate with engine
//have handlers whatever

namespace ChessEngine
{
    public class ChessEngineSystem : IDisposable
    {       
        
       
        
        public static ChessEngineSystem Instance { get; private set; }
        private Board board = new Board();
        private BotBrain bot1 = new BotBrain();
        private BotBrain bot2 = new BotBrain();
        private readonly int boardSetupDelay = 50;
        private readonly int botDecisionDelay = 50;
        private Stack<ICommand> moveHistory = new Stack<ICommand>();
        private bool startingNewBoard = true;

        private bool isUndoRequest;


       
        private GameStateManager gameStateManager ;

        public GameStateManager GetGameStateManager => gameStateManager;
        
        public ChessEngineSystem()
        {
            Console.WriteLine("Console initialized");
            Event.inComingData += PassDataToBoard;
            Event.GetCellsForThisIndex += SendUICellIndicatorData;
            Event.undoMove  += UndoCommand;
        }
        static ChessEngineSystem()
        {
            Instance = new ChessEngineSystem();
        
        }

        public int[] MapFen() => FenMapper.MapFen();
       
        
        public void Init()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" --> Simple Chess Engine 0.1 <--" );
            Connection.Instance.Init(); 
            gameStateManager = new GameStateManager();
            Console.ResetColor();
        }
        
        
        public async Task  CheckForGameModeAndPerform()
        {

            if (startingNewBoard)
            {
                await Task.Delay(boardSetupDelay);
                startingNewBoard = false;
            }
            
            switch (GameStateManager.Instance.GetCurrentGameMode)
            {   
                // can use  diff bot for diff things
                case GameMode.PlayerVsBot :
                    if ( GameStateManager.Instance.player1MoveCol == GameStateManager.Instance.playerToMove) // for the first turn
                    {   
                        Console.WriteLine("Bot is gonna make the move");
                        bot1.Think(ref GameStateManager.Instance.allPiecesThatCanMove);
                        ScanBoardForMoves();
                    }
                    break;
                
                
                case GameMode.BotVsBot :
                    
                    if ( GameStateManager.Instance.player1MoveCol == GameStateManager.Instance.playerToMove) // for the first turn
                    {
                        BotMove(ref bot1);


                    }
                    else if ( GameStateManager.Instance.player2MoveCol == GameStateManager.Instance.playerToMove) // for the first turn
                    {   
                       
                     BotMove(ref bot2);
                       
                    }
                    
                    break;
                case GameMode.PlayerVsPlayer :
                    break;
            }
        }


        private void BotMove(ref BotBrain brain)
        {
            Console.WriteLine($"Bot {GameStateManager.Instance.playerToMove.ToString()}  is  gonna make the move , Waiting ...");
            Thread.Sleep(botDecisionDelay);
            brain.Think(ref GameStateManager.Instance.allPiecesThatCanMove);
            ScanBoardForMoves();
            CheckForGameModeAndPerform();
        }

        //This will unwrap the data and send to the board 
        private void PassDataToBoard(string data)
        {   
              //   Console.WriteLine("Received move data");
                string validationData = ProcessMoveInEngine(data) ? "true" : "false";
                Protocols finalData = new Protocols(ProtocolTypes.VALIDATE.ToString(), validationData, GameStateManager.Instance.GetTurnToMove.ToString());
                SendDataToUI(finalData);
                ScanBoardForMoves();
                //After we performed moves -> we get if it is validated and the above things happen as usual 
                CheckForGameModeAndPerform();

            
        }

     




        //convert notations to the board index
        private bool ProcessMoveInEngine(string incomingDta)
        {
            
          //  var (oldIndex, newIndex) = FenMapper.AlgebricToBoard(square); -> h2h3

          int currentSquare = int.Parse( incomingDta.Split("-")[0]);
          int targetSquare = int.Parse( incomingDta.Split("-")[1]);
          
            //TODO : AT THE END , ITS JUST INDEX THATS GIVEN
            return board.MakeMove(currentSquare, targetSquare );
        }
        
        
        //SENDS THE UI UPDATE FOR POST PERFORM -> CASTLING , EN PASSANT,  PROMOTION
        //USED BY BOTS
        public void UpdateUIWithNewIndex(int oldIndex , int newIndex)
        {
            List<int> update = new List<int>() { oldIndex, newIndex };
            var data = JsonConvert.SerializeObject(update);
            Protocols finalData = new Protocols(ProtocolTypes.UPDATEUI.ToString() , data , GameStateManager.Instance.playerToMove.ToString());
            SendDataToUI(finalData);
        }
        //Exclusively used for Undoing - REcived
        public void UpdateUIWithNewIndex(int oldIndex , int newIndex , int capturedPiece)
        {
            List<int> update = new List<int>() { oldIndex, newIndex , capturedPiece};
            var data = JsonConvert.SerializeObject(update);
            Protocols finalData = new Protocols(ProtocolTypes.UPDATEUI.ToString() , data , GameStateManager.Instance.playerToMove.ToString() );
            SendDataToUI(finalData);
          
        }

        
        
        
        //BASICALLY , JUST DO POST FORMAT FOR MOVES CASTLING, PROMOTION ETC
        //RECEIVE
        private void SendUICellIndicatorData(int index)
        {
           
            HashSet<int> cellsToSend = new HashSet<int>();
            foreach (var piece in GameStateManager.Instance.allPiecesThatCanMove) {
                if (index == piece.GetCurrentIndex) {
                    foreach (var p in piece.GetAllMovesForThisPiece) {
                        cellsToSend.Add(p);
                        
                    }
                    break; 
                }
            }

            var setData = JsonConvert.SerializeObject(cellsToSend);
            Protocols finalData = new Protocols(ProtocolTypes.INDICATE.ToString() , setData, null);
            SendDataToUI(finalData);
            foreach (var cell in  cellsToSend)
            {
                Console.WriteLine(cell);
                
            }
        }

        public void Dispose()
        {
            Event.inComingData -= PassDataToBoard;
            Event.GetCellsForThisIndex -= SendUICellIndicatorData;
            Event.undoMove -= UndoCommand;
            board.Dispose();
        }


       public void SendDataToUI <T>(T data)
       {
          
            string toSend = JsonConvert.SerializeObject(data);
            Connection.Instance.Send(toSend);
        }

       public bool IsPawnDefIndex(int pawn)  =>board.CheckIfPawnDefaultIndex(pawn);


       public void ScanBoardForMoves()
       {
           board.ProcessMovesUpdate();
       }
        
       public void CustomScanBoardForMoves(int[] testBoard , int toMoveColour)
       {
           board.ProcessMovesUpdate(testBoard , toMoveColour);
       }

       public bool IsBlack(int colorCode)
       {
           return (colorCode & Piece.Black) == Piece.Black;
       }

       public int GetPieceCode(int code)
       {
           return (code & Piece.CPiece);
       }

       public Board GetBoardClass => board;

       
       //USE THIS FOR ANY BOT UI UPDATE


      //Pawn , Bishop , Queen , Rook , Kings , Knight
       public void ExecuteCommand(ICommand move)
       {      
            moveHistory.Push(move);
            move.Execute();
            
       }


        /// <summary>
        /// Event for Ui for Undo just a simple event
        /// </summary>
       private void UndoCommand(string data)
        {
            if (moveHistory.Count == 0) return; // no  more moves to make
            GameStateManager.Instance.UpdateTurns();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.ResetColor();
            ICommand lastMove = moveHistory.Pop();
            lastMove.Undo();
            Console.WriteLine($"Received Undo Command showing board  , { GameStateManager.Instance.playerToMove} has to move again ABEG");
            GameStateManager.Instance.ResetMoves();
            ScanBoardForMoves();
            CheckForGameModeAndPerform();
            


        }
      
    }

}