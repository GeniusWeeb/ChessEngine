
using Utility;
using Newtonsoft.Json;

namespace ChessEngine 
{
    // NOTE  :  This will  Handle BOARD CREATION 

 public class Board : IDisposable
 {
     private  int[] chessBoard;
     
     public Board()
     {
         chessBoard = new int[64];
         Console.WriteLine("Board is ready!");
         Event.ClientConncted += SetupDefaultBoard;
         
     }
    
     

     void SetupDefaultBoard()
     {
         chessBoard = ChessEngineSystem.Instance.MapFen();
         var data = JsonConvert.SerializeObject(chessBoard);
         Console.WriteLine(data);
         Connection.Instance.Send(data);
       
     }

     public void MakeMove(int piece, int oldIndex, int newIndex)
     {
         //swap
         chessBoard[oldIndex] = Piece.Empty;
         chessBoard[newIndex] = piece;
         ShowBoard();
     }


     private void ShowBoard()
     {
         Console.WriteLine(JsonConvert.SerializeObject(chessBoard));
     }


     public void Dispose()
     {
         Event.ClientConncted -= SetupDefaultBoard;

     }
 }   


}
