
using ChessEngine;
using Utility;
using Python.Runtime;



//Console Entry point
// Entry point for the program else
public class Program
{

    static void Main()
    {
       
       // Runtime.PythonDLL = "/Users/zimjanmol/miniforge3/envs/ml/lib/libpython3.9.dylib";
        ChessEngineSystem.Instance.Init();
      
        
        
        
         // this will also creat the static instance of the class on the go
         // We dont need to do  ,  Connection conn =  new Connection();
        
         AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
         { 
             ChessEngineSystem.Instance.Dispose();
         };

         
         Console.ReadLine();
         
    }

 
    
}





