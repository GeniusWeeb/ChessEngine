using System.Diagnostics;
using Utility;

// This class will communicate with engine
//have handlers whatever

namespace ChessEngine
{
    public class ChessEngineSystem
    {
        private Board b = new Board();
        public ChessEngineSystem()
        {
            Console.WriteLine("Console initialized");
            Init();
        }
        private void Init()
        {
            Console.WriteLine(" --> Simple Chess Engine 0.1 <--" );
        }   
        
        
    }

}