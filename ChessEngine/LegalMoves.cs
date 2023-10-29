namespace ChessEngine;


// This is a set in stone , defined file . 

sealed class LegalMoves
{   
    
    // need to run like a time check on how long this takes to run and-
    // further optimize it
    private void CheckForMoves(int[] board)
    {

        for (int i = 0; i < board.Length; i++)
        {   //i -> index
            int pieceCode = 00; // extra piece code and colour code from here
            int colorCode;

            if (pieceCode == Piece.Pawn)
            {
                GenerateMovesForPawn(i, pieceCode , board);
            }
        }

    }

    //going thru every peice
    private ChessPiece GenerateMovesForPawn(int ind, int p , int[] board)
    {   
        int index = ind;
        int pCode = p;
        int colorCode;
        Pawn pawn = new Pawn(pCode);
        
        //Start testing for every possible moves without any problems .
        //only discount invalid moves , NOT ILLEGAL MOVES . 
        
        //front move -> normal move
        if(board[index + pawn.pawnStep] == Piece.Empty)
            pawn.SetAllPossibleMoves(index+ pawn.pawnStep);
        
        //RIGHT MOVE
        //if(board[index + pawn.pawnStep +1 ] == (Piece.Black | ))
            
            
        return pawn;
        
    }

}

public class Pawn : ChessPiece
{
    public  readonly int pawnStep = 8;
    public Pawn(int pCode )
    {
        this.pieceCode = pCode;
    }
    public int GetPieceCode => pieceCode;
    public void SetAllPossibleMoves(int index)
    {
        allPossibleMovesIndex.Add(index);
        
    }
}


public abstract class ChessPiece
{   
    protected int pieceCode { get; init; }
    protected HashSet<int> allPossibleMovesIndex = new HashSet<int>();
    

}
