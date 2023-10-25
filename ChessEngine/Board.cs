
using Utility;

namespace ChessEngine 
{
    // NOTE  :  This will  Handle BOARD CREATION 

 public class Board : IDisposable
 {
     private  int[] square;
     
     public Board()
     {
         square = new int[64];
         Console.WriteLine("Board is also ready");
         Event.ClientConncted += SpawnPiece;
         
     }
    
     

     void SpawnPiece()
     {
         square[3] = Piece.Black |  Piece.Queen;
         Console.WriteLine("Sending peice data ");
         Connection.Instance.Send(square[3].ToString());
       
     }


     public void Dispose()
     {
         Event.ClientConncted -= SpawnPiece;

     }
 }   


}
