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
                board.ProcessMovesUpdate();
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

      
        
        
    }

}