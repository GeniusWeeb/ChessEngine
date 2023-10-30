
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
    
     
     

    private void SetupDefaultBoard()
     {
         chessBoard = ChessEngineSystem.Instance.MapFen();
         var data = JsonConvert.SerializeObject(chessBoard);
         Protocols finalData = new Protocols(ProtocolTypes.GAMESTART.ToString(),data ,16.ToString());
         Console.WriteLine(data);
         ChessEngineSystem.Instance.SendDefaultBoardData(finalData);
         
         //after turn change TBH
         
         Console.WriteLine("Checking for all Pawn moves");
         GameStateManager.Instance.ProcessMoves(chessBoard);
       
     }

    public bool MakeMove(int piece, int oldIndex, int newIndex)
    {   
        Console.WriteLine($"piece {piece} and old index {oldIndex} and new index {newIndex}");
        foreach (var p in GameStateManager.Instance.allPiecesThatCanMove)
        {
            
            if (p.GetPieceCode == piece && p.GetAllMovesForThisPiece.Contains(newIndex) &&
                oldIndex == p.GetCurrentIndex)
            {

                Console.WriteLine("Piece that moved" + piece + "with old index -> " + oldIndex + " and new index ->" +
                                  newIndex);
                chessBoard[oldIndex] = Piece.Empty;
                chessBoard[newIndex] = piece;
                ShowBoard();

            //    GameStateManager.Instance.UpdateTurns(GameStateManager.Instance.GetTurnToMove);
                GameStateManager.Instance.ResetMoves();
                return true;
                ; }

            

        }

        Console.WriteLine("Invalid Move");
            return false;
     }

   

    private void ShowBoard()
     {
         Console.WriteLine(JsonConvert.SerializeObject(chessBoard));
     }


     public void Dispose()
     {
         Event.ClientConncted -= SetupDefaultBoard;

     }


     public void ProcessMovesUpdate()
     {
         GameStateManager.Instance.ProcessMoves(chessBoard);
     }
 }   


}
