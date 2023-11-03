using System.Diagnostics;
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
        
        public void Init()
        {
            Console.WriteLine(" --> Simple Chess Engine 0.1 <--" );
        }
        
        //This will unwrap the data and send to the board 
        private void PassDataToBoard(string data)
        {   
            Protocols incomingData = JsonConvert.DeserializeObject<Protocols>(data);
          //  Console.WriteLine(incomingData.msgType);
            if (incomingData.msgType == ProtocolTypes.MOVE.ToString())
            {   
              //   Console.WriteLine("Received move data");
                string validationData = ProcessMoveInEngine(incomingData) ? "true" : "false";

                Protocols finalData = new Protocols(ProtocolTypes.VALIDATE.ToString() ,validationData , GameStateManager.Instance.GetTurnToMove.ToString());
               // Console.WriteLine("Can make this move =>" + validationData);

               SendDataToUI(finalData);
               ScanBoardForMoves();
                
            }
        }

        private bool ProcessMoveInEngine(Protocols incomingDta)
        {
            string square = incomingDta.data.Split("-")[1];
            int pieceType = FenMapper.GetPieceCode((incomingDta.data.Split("-")[0]).Single());
            int pieceColor = char.IsUpper((incomingDta.data.Split("-")[0]).Single()) ? Piece.White : Piece.Black;

            int piece = pieceType | pieceColor;
            var (oldIndex, newIndex) = FenMapper.AlgebricToBoard(square);
            
            //can the move be made 
            return board.MakeMove(piece ,oldIndex, newIndex );
        }
        
        
        //CustomMoveInput
        public bool ProcessMoveInEngine(string incomingDta)
        {
            // need some changes for capture
            string square = incomingDta.Split("-")[1];
            int pieceType = FenMapper.GetPieceCode((incomingDta.Split("-")[0]).Single());
            int pieceColor = char.IsUpper((incomingDta.Split("-")[0]).Single()) ? Piece.White : Piece.Black;

            int piece = pieceType | pieceColor;
            var (oldIndex, newIndex) = FenMapper.AlgebricToBoard(square);
            
            //can the move be made 
            return board.MakeMove(piece ,oldIndex, newIndex );
        }

        
        
        //RECIEVE
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
       public void UpdateUIWithNewIndex(int oldIndex , int newIndex)
       {
           List<int> update = new List<int>() { oldIndex, newIndex };
           var data = JsonConvert.SerializeObject(update);
           Protocols finalData = new Protocols(ProtocolTypes.UPDATEUI.ToString() , data , GameStateManager.Instance.GetTurnToMove.ToString());
           SendDataToUI(finalData);
       }
    }

}