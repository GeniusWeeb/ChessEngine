using System.Diagnostics;
using Utility;

// This class will communicate with engine
//have handlers whatever

namespace ChessEngine
{
    public class ChessEngineSystem : IDisposable
    {
        private Board b = new Board();
        
        public ChessEngineSystem()
        {
            Console.WriteLine("Console initialized");
            Event.inComingData += PassDataToBoard;
            Init();
        }
        
        private void Init()
        {
            Console.WriteLine(" --> Simple Chess Engine 0.1 <--" );
        }
        
        //This will unwrap the data and send to the board 
        private void PassDataToBoard(string data)
        {
            Console.WriteLine(data);
        }
        
        public void Dispose()
        {
            Event.inComingData -= PassDataToBoard;
        }
    }

}