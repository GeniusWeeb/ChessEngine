using System.ComponentModel.Design;
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

        public string TestFen = "rnbqkbnr/pppppppp/8/8/1P6/8/P1PPPPPP/RNBQKBNR b KQkq b3 0 1";
        public static ChessEngineSystem Instance { get; private set; }
        private Board? board = new Board();
        private BotBrain? bot1 = new BotBrain();
        private BotBrain? bot2 = new BotBrain();
        private readonly int boardSetupDelay = 50;
        private readonly int botDecisionDelay = 50;
        public  Stack<ICommand> moveHistory = new Stack<ICommand>();
       
        private bool newServerInstance = true;
        private bool startingNewBoard = true;

        private bool isUndoRequest;


        private PerfTest test = new PerfTest();
        
        public ChessEngineSystem()
        {
            
            Console.WriteLine("Console initialized");
           
            Event.inComingData += PassDataToBoard;
            Event.GetCellsForThisIndex += SendUICellIndicatorData;
            Event.undoMove  += UndoCommand;
            Event.ClientConncted += SetupDefaultBoard;
        }
        static ChessEngineSystem()
        {
            Instance = new ChessEngineSystem();
        
        }

        public int[] MapFen() => FenMapper.MapFen(TestFen);
       
        
        public void Init()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" --> Simple Chess Engine 0.1 <--" );
            Connection.Instance.Init(); 
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
                        bot1.Think();
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
                
                case GameMode.PerfTest:
                    test.PerFMoveFinal();
                    break;
                    
            }
        }


        private void BotMove(ref BotBrain brain)
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------------------------------------------------------------------");
                Console.WriteLine($"Bot {GameStateManager.Instance.playerToMove.ToString()}  is  gonna make the move , Waiting ...");
            Console.ResetColor();
            Thread.Sleep(botDecisionDelay);
            
            //main issue is here
            ScanBoardForMoves();
            UtilityWriteToConsoleWithColor("Scanning Finished  bot gonna think", ConsoleColor.Red);
            
            brain.Think();          
            UtilityWriteToConsoleWithColor("Bots thinking finished", ConsoleColor.DarkGreen);

            CheckForGameModeAndPerform();
            
            //Fix this
        }

        //This will unwrap the data and send to the board 
        private void PassDataToBoard(string data)
        {   
              //   Console.WriteLine("Received move data");
                string validationData = ProcessMoveInEngine(data) ? "true" : "false";
                Protocols finalData = new Protocols(ProtocolTypes.VALIDATE.ToString(), validationData, GameStateManager.Instance.GetTurnToMove.ToString());
                SendDataToUI(finalData);
                
                if(GameStateManager.Instance.GetCurrentGameMode != GameMode.BotVsBot)
                    ScanBoardForMoves();
                
                //After we performed moves -> we get if it is validated and the above things happen as usual 
                CheckForGameModeAndPerform();

            
        }

        public void UtilityWriteToConsoleWithColor(string content, ConsoleColor color = ConsoleColor.Red)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            Console.ResetColor();
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
            
         //EnPassant 
        }  public void UpdateUIWithNewIndex(int oldIndex , int newIndex , int capturedPiece , int capturedPawnIndex)
        {
            List<int> update = new List<int>() { oldIndex, newIndex , capturedPiece, capturedPawnIndex};
            var data = JsonConvert.SerializeObject(update);
            Protocols finalData = new Protocols(ProtocolTypes.UPDATEUI.ToString() , data , GameStateManager.Instance.playerToMove.ToString() );
            SendDataToUI(finalData);
          
        }
        public void UpdateUIWithNewIndex(int singleIndexToRemove)
        {
            List<int> update = new List<int>() { singleIndexToRemove };
            var data = JsonConvert.SerializeObject(update);
            Protocols finalData = new Protocols(ProtocolTypes.UPDATEUI.ToString() , data , GameStateManager.Instance.playerToMove.ToString());
            SendDataToUI(finalData);    
        }
        


        private void SetupDefaultBoard(string gameMode)
        {
            if (newServerInstance)
            {
                board.SetupDefaultBoard(gameMode);
                newServerInstance = false; 
            }
            else
            {
                ReloadEngine();
                board.SetupDefaultBoard(gameMode);
            }
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
           
        }

        
        public void Dispose()
        {
            Event.inComingData -= PassDataToBoard;
            Event.GetCellsForThisIndex -= SendUICellIndicatorData;
            Event.undoMove -= UndoCommand;
            Event.ClientConncted -= SetupDefaultBoard;
            
        }

      


        public void SendDataToUI <T>(T data)
       {
          
            string toSend = JsonConvert.SerializeObject(data);
            Connection.Instance.Send(toSend);
        }

       public bool IsPawnDefIndex(int pCode, int index)  =>board.CheckIfPawnDefaultIndex(pCode, index);


       
       //Main Scan
       public void ScanBoardForMoves()
       {
            
           Console.ForegroundColor = ConsoleColor.Yellow;
         //  Console.WriteLine(" Main Scanning board");
           Console.ResetColor();
           board.ProcessMovesUpdate();
           GetBoardClass.KingBePreCheckTest(  board.chessBoard, GameStateManager.Instance.playerToMove);
       }
       
       
     
   
       
       // Used for any precomputes
       public void CustomScanBoardForMoves(int[] testBoard , int toMoveColour , string reason)
       {    
           Console.ForegroundColor = ConsoleColor.Red;
          // Console.WriteLine( $"{GameStateManager.Instance.GetTurnToMove } is custom Scanning board for {reason} ");
           Console.ResetColor();
           try
           {
               board.ProcessMovesUpdate(testBoard , toMoveColour);
           }
           catch (Exception e)
           {
               Console.WriteLine(e);
               throw;
           }
           
            
       }

       public bool IsBlack(int colorCode)
       {
           return (colorCode & Piece.Black) == Piece.Black;
       }

       public int GetPieceCode(int code)
       {
           return (code & Piece.CPiece);
       }
       public  int GetColorCode(int code )
       {
           if (IsBlack(code))
               return Piece.Black;
        
           return (code & Piece.White) == Piece.White ? Piece.White : Piece.Empty;
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Received Undo Command showing board\n");
            Console.ResetColor();
            board.ShowBoard();
            GameStateManager.Instance.ResetMoves();
            ScanBoardForMoves();
            CheckForGameModeAndPerform();

        }
        
        
        // private void UndoCommand(string data)
        // {
        //     if (moveHistory.Count == 0) return; // no  more moves to make
        //     GameStateManager.Instance.UpdateTurns();
        //     Console.ForegroundColor = ConsoleColor.Cyan;
        //     Console.ResetColor();
        //     ICommand lastMove = moveHistory.Pop();
        //     lastMove.Undo();
        //     Console.ForegroundColor = ConsoleColor.Red;
        //     Console.WriteLine($"Received Undo Command showing board\n");
        //     Console.ResetColor();
        //     board.ShowBoard();
        //     GameStateManager.Instance.ResetMoves();
        //     ScanBoardForMoves();
        //     CheckForGameModeAndPerform();
        //
        // }

        public void ReloadEngine()
        { 
            board = null;
            bot1 = null;
            bot2 = null;
        Board newBoard = new Board();
        BotBrain newBot1 = new BotBrain();
        BotBrain newBot2 = new BotBrain();
         board = newBoard;
         bot1 = newBot1;
         bot2 = newBot2;
         moveHistory.Clear();
         startingNewBoard = true;
         GameStateManager.Instance.ResetGameState();
       
         
        }

        
        
    }

}