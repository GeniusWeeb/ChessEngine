
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
       //  Console.WriteLine(data);
         ChessEngineSystem.Instance.SendDataToUI(finalData);
         
         //after turn change TBH
         
       
         GameStateManager.Instance.ProcessMoves(chessBoard);
       
     }
    
    public bool MakeMove(int piece, int oldIndex, int newIndex)
    {   
        //Console.WriteLine($"piece {piece} and old index {oldIndex} and new index {newIndex}");
        foreach (var p in GameStateManager.Instance.allPiecesThatCanMove)
        {
            
            if (p.GetPieceCode == piece && p.GetAllMovesForThisPiece.Contains(newIndex) &&
                oldIndex == p.GetCurrentIndex)
            {
               // Console.WriteLine("Piece that moved" + piece + "with old index -> " + oldIndex + " and new index ->" +newIndex);
                CheckForBonusBasedOnPieceCapture(piece,chessBoard[newIndex]);
                chessBoard[oldIndex] = Piece.Empty;
                chessBoard[newIndex] = piece;
                ShowBoard();
                GameStateManager.Instance.ResetMoves();
                GameStateManager.Instance.UpdateTurns(GameStateManager.Instance.toMove);
                return true;
                ; }
        }

        Console.WriteLine("Invalid Move");
            return false;
     }

    private void CheckForBonusBasedOnPieceCapture(int pieceThatMoved, int newIndex)
    
    {
        int pCode =newIndex & Piece.CPiece;
       // Console.WriteLine("PCode is that got captured" + pCode);
       // Console.WriteLine("Chessboard Index" + newIndex);
        
        switch (pCode)
        {
            case 0: 
               // Console.WriteLine("0 point for Empty");
                break;
            case 1: 
               // Console.WriteLine("1 point for Pawn");
                break;
            case 2:
               // Console.WriteLine("2 points for Rook");
                break;
            case 3: 
               // Console.WriteLine("3 points for Knight");
                break;
            case 4: 
               // Console.WriteLine("4 points for Bishop");
                break;
            case 6:// Console.WriteLine("6 points for Queen");
                break;
            
            
        }
    }


    public void CheckForBonusBasedOnPieceCapture()
    {
        
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
