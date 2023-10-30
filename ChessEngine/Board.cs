
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
         Protocols finalData = new Protocols(ProtocolTypes.GAMESTART.ToString(),data ,GameConstants.defaultMove.ToString());
         Console.WriteLine(data);
         ChessEngineSystem.Instance.SendDefaultBoardData(finalData);
       
     }

     public bool MakeMove(int piece, int oldIndex, int newIndex)
     {  
         
         //FOR TESTING
         bool canMakeMove = false;
         //swap
         chessBoard[oldIndex] = Piece.Empty;
         chessBoard[newIndex] = piece;
         ShowBoard();
        //validate  the move here
        //can we make a move ?
         return canMakeMove;
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
