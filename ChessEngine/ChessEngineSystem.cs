using System.Diagnostics;
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
            Protocols incomingData = Newtonsoft.Json.JsonConvert.DeserializeObject<Protocols>(data);
            if (incomingData.msgType == ProtocolTypes.MOVE.ToString())
                ProcessMoveInEngine(incomingData);
        }

        private void ProcessMoveInEngine(Protocols incomingDta)
        {
            string square = incomingDta.data.Split("-")[1];
            int pieceType = FenMapper.GetPieceCode((incomingDta.data.Split("-")[0]).Single());
            int pieceColor = char.IsUpper((incomingDta.data.Split("-")[0]).Single()) ? Piece.White : Piece.Black;

            int piece = pieceType | pieceColor;
            var (oldIndex, newIndex) = FenMapper.AlgebricToBoard(square);
            board.MakeMove(piece ,oldIndex, newIndex );
           
        }

        public void Dispose()
        {
            Event.inComingData -= PassDataToBoard;
            board.Dispose();
        }
    }

}