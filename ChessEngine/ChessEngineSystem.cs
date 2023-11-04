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
        private BotBrain bot = new BotBrain();
        
        public ChessEngineSystem()
        {
            Console.WriteLine("Console initialized");
            Event.inComingData += PassDataToBoard;
            Event.GetCellsForThisIndex += SendUICellIndicatorData;
        }
        static ChessEngineSystem()
        {
            Instance = new ChessEngineSystem();
        }

        public int[] MapFen() => FenMapper.MapFen();
        public Board getBoard => board;
        
        public void Init()
        {
            Console.WriteLine(" --> Simple Chess Engine 0.1 <--" );
        }
        
        
        public void CheckForGameModeAndPerform()
        {
        
            
            switch (GameStateManager.Instance.GetCurrentGameMode)
            {
                case GameMode.PlayerVsBot :
                    if ( GameStateManager.Instance.player1Move != GameStateManager.Instance.player2Col) // for the first turn
                    {   
                        Console.WriteLine("Bot is gonna make the move");
                        bot.Think(ref GameStateManager.Instance.allPiecesThatCanMove);
                    }
                    break;
                case GameMode.PlayerVsPlayer :
                    break;
            }
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
            Protocols finalData = new Protocols(ProtocolTypes.UPDATEUI.ToString() , data , GameStateManager.Instance.GetTurnToMove.ToString());
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
        }

        public void Dispose()
        {
            Event.inComingData -= PassDataToBoard;
            Event.GetCellsForThisIndex -= SendUICellIndicatorData;
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
       
       //USE THIS FOR ANY BOT UI UPDATE
      
    }

}