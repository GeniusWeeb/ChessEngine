using Utility;

namespace ChessEngine;


// This is a set in stone , defined file . 

sealed class LegalMoves
{   
    
    // need to run like a time check on how long this takes to run and-
    // further optimize it
    public void CheckForMoves(int[] board  , int colorToMove)
    {
        
        for (int i = 0; i < board.Length; i++)
        {
            Console.WriteLine("Current color code is " + colorToMove);
            int pieceCode = board[i] & Piece.CPiece;
            int colorCode = isBlack(board[i]) ? Piece.Black : Piece.White;
            if (colorCode != colorToMove)
               continue;
            
            if (pieceCode == Piece.Pawn)
            {
                ChessPiece p =   GenerateMovesForPawn(i, board[i] , colorCode,board);
              if(p != null)
                  GameStateManager.Instance.allPiecesThatCanMove.Add(p);
            }
        }

    }

    //going thru every peice
    private ChessPiece GenerateMovesForPawn(int ind, int p ,int colCode, int[] board)
    {   
        
        int index = ind;
        int pCode = p;
        int thisColorCode = colCode;
        Pawn pawn = new Pawn(pCode, ind);
        
       
        var ApplyIndexBasedOnColor = colCode == Piece.White ? index + pawn.pawnStep : index - (pawn.pawnStep);
        //front move -> normal move
        if(board[ApplyIndexBasedOnColor] == Piece.Empty)
            pawn.SetAllPossibleMoves(ApplyIndexBasedOnColor);

        int targetRow = ( ApplyIndexBasedOnColor )/ 8; // check if all moves are in same row ,front and front sides (Diagonals)
        //RIGHT MOVE
        int rdIndex = ApplyIndexBasedOnColor + 1;
     
        int RDcolorCode = isBlack(board[rdIndex]) ? Piece.Black : Piece.White;
        if (rdIndex / 8 == targetRow && thisColorCode != RDcolorCode &&  board[rdIndex] != Piece.Empty )
        { ;
            pawn.SetAllPossibleMoves(rdIndex);
        }

        int ldIndex = ApplyIndexBasedOnColor - 1;
        int LDcolorCode = isBlack(board[ldIndex]) ? Piece.Black : Piece.White;
        if (ldIndex / 8 == targetRow && thisColorCode != LDcolorCode && board[ldIndex] != Piece.Empty)
        {   
            
            pawn.SetAllPossibleMoves(ldIndex);
        }


        return pawn.getAllPossibleMovesCount > 0 ? pawn : null;
    }

    private bool isBlack(int pCode)
    {
        return (pCode & Piece.Black) == Piece.Black;
    }


}

public class Pawn : ChessPiece
{
    public  readonly int pawnStep = 8;
    public Pawn(int pCode , int index)
    {
        this.pieceCode = pCode;
        this.currentIndex = index;
    }
  
    public void SetAllPossibleMoves(int index)
    {
        allPossibleMovesIndex.Add(index);
        
    }
    public int getAllPossibleMovesCount => allPossibleMovesIndex.Count;
}


public abstract class ChessPiece
{   
    
    protected int pieceCode { get; init; }
    protected HashSet<int> allPossibleMovesIndex = new HashSet<int>();
    protected int currentIndex { get; init; }
    
    public int GetPieceCode => pieceCode;
    public int GetCurrentIndex => currentIndex;
    public HashSet<int> GetAllMovesForThisPiece => allPossibleMovesIndex;
}
