using System.Diagnostics;

// These are class library files
//  
namespace ChessEngine
{
public class ChessEngineSystem
{

   private  BoardRepresentation board  = new BoardRepresentation();
    public ChessEngineSystem()
    {
        Console.WriteLine("Console initialized");
        Init();
        board.CreateChessBoard();
    
        
    }


    private void Init()
    {
        Console.WriteLine(" --> Simple Chess Engine 0.1 <--" );
    }
}

}