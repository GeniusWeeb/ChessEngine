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
            Console.WriteLine(incomingData.msgType);
            if (incomingData.msgType == ProtocolTypes.MOVE.ToString())
            {   
                Console.WriteLine("Recieved move data");
                string validationData = ProcessMoveInEngine(incomingData) ? "true" : "false";
                Protocols finalData = new Protocols(ProtocolTypes.VALIDATE.ToString() ,validationData);
                Console.WriteLine("Can make this move =>" + validationData);
                SendDataAfterValidating(finalData);
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

        public void Dispose()
        {
            Event.inComingData -= PassDataToBoard;
            board.Dispose();
        }


        void SendDataAfterValidating <T>(T data)
        { 
            string toSend = JsonConvert.SerializeObject(data);
            Connection.Instance.Send(toSend);
        }

        public void SendDefaultBoardData<T>(T data)
        {
            var toSend = JsonConvert.SerializeObject(data);
            Connection.Instance.Send(toSend);
        }
    }

}