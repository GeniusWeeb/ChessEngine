
using ChessEngine;
using Utility;



//Console Entry point
// Entry point for the program else
public class Program
{

    static void Main()
    {
        
         ChessEngineSystem chessEng = new ChessEngineSystem();
         Connection.Instance.Init(); // this will also creat the static instance of the class on the go
         // We dont need to do  ,  Connection conn =  new Connection();
        
         AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
         { 
             chessEng.Dispose();
         };
         
         Console.ReadLine();
         
    }

 
    
}





