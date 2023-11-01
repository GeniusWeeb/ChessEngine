using System.Diagnostics;
using Utility;

namespace ChessEngine;


// This is a set in stone , defined file . 

sealed class LegalMoves
{
    private Stopwatch watch = new Stopwatch();
    // need to run like a time check on how long this takes to run and-
    // further optimize it
    public void CheckForMoves(int[] board  , int colorToMove)
    {
        
        watch.Start();
        for (int i = 0; i < board.Length; i++)
        {
            if(board[i] ==  Piece.Empty)
             continue;

           // Console.WriteLine("Current color code is " + colorToMove);
            int pieceCode = board[i] & Piece.CPiece;
            int colorCode = GetColorCode(board[i]);
            if (colorCode != colorToMove)
               continue;

            //TODO : CAN USE SWITCH CASE HERE
            
            if (pieceCode == Piece.Pawn)
            {
                ChessPiece p =  GenerateMovesForPawn(i, colorCode,board);
                if(p != null) GameStateManager.Instance.allPiecesThatCanMove.Add(p);
            }
             if (pieceCode == Piece.Knight)
            {
              ChessPiece p = GenerateMovesForKnight(i , colorCode,board);
              if(p!=null)  GameStateManager.Instance.allPiecesThatCanMove.Add(p);

            }  if (pieceCode == Piece.Queen)
            {
                ChessPiece p = GenerateMovesForQueen(i , colorCode,board);
                if(p!=null)  GameStateManager.Instance.allPiecesThatCanMove.Add(p);
            }

        }
        watch.Stop();
        Console.WriteLine( $"Time it take to iterate thru the board{watch.Elapsed}");
        watch.Reset();
    }

    //going thru every  piece
    private ChessPiece GenerateMovesForPawn(int ind,int colCode, int[] board)
    {
        
        //TODO : PREDEFINE MOVEMENT STEPS IN CONSTRUCTOR BASED ON BLACK AND WHITE
        int index = ind;
        int thisColorCode = colCode;
        Pawn pawn = new Pawn(thisColorCode, ind);
        
       
        var ApplyIndexBasedOnColor = colCode == Piece.White ? index + pawn.pawnStep : index - (pawn.pawnStep);
        //front move -> normal move
        if(board[ApplyIndexBasedOnColor] == Piece.Empty)
            pawn.AddAllPossibleMoves(ApplyIndexBasedOnColor);

        int targetRow = ( ApplyIndexBasedOnColor )/ 8; // check if all moves are in same row ,front and front sides (Diagonals)
        //RIGHT MOVE
        int rdIndex = ApplyIndexBasedOnColor + 1;
     
        int RDcolorCode = isBlack(board[rdIndex]) ? Piece.Black : Piece.White;
        if (rdIndex / 8 == targetRow && thisColorCode != RDcolorCode &&  board[rdIndex] != Piece.Empty )    
            pawn.AddAllPossibleMoves(rdIndex);

        int ldIndex = ApplyIndexBasedOnColor - 1;
        int LDcolorCode = isBlack(board[ldIndex]) ? Piece.Black : Piece.White;
        if (ldIndex / 8 == targetRow && thisColorCode != LDcolorCode && board[ldIndex] != Piece.Empty)
            pawn.AddAllPossibleMoves(ldIndex);


        return pawn.getAllPossibleMovesCount > 0 ? pawn : null;
    }

    private ChessPiece GenerateMovesForKnight(int ind, int thisColCode, int[] board)
    {
        int index = ind;
        int mycolCode = thisColCode;
        Knight knight = new Knight(mycolCode, index);
        int movesLength = knight.knightCanMoveTo.Length;
        for (int i = 0; i < movesLength; i++)
        {   
            if( knight.knightCanMoveTo[i] <= 0 || knight.knightCanMoveTo[i] > 63)
                continue;
            
            int otherColorCode = GetColorCode(knight.knightCanMoveTo[i]);
            bool sameColor = isSameColorAsMe(mycolCode, otherColorCode);

            if (board[knight.knightCanMoveTo[i]] == Piece.Empty)
            {
                
                int newPos =knight.knightCanMoveTo[i];
                //After having all moves  , we try to check for L move shape 
                //INTERESTING LOGIC FOR THIS -> coz it always moves 2 spaces in 1 dir and 1 dir perpendicular
                int rowDiff = (int)MathF.Abs(newPos / 8 - ind / 8);
                int colDiff = (int)MathF.Abs(newPos % 8 - ind % 8);
                
                if((rowDiff ==2 && colDiff == 1) || (rowDiff==1 && colDiff==2))
                    knight.AddAllPossibleMoves(newPos);
            }
            else if (!sameColor)
            {
                int newPos = board[knight.knightCanMoveTo[i]];
                //After having all moves  , we try to check for L move shape 
                //INTERESTING LOGIC FOR THIS
                int rowDiff = (int)MathF.Abs(newPos / 8 - ind / 8);
                int colDiff = (int)MathF.Abs(newPos % 8 - ind % 8);
                
                if((rowDiff ==2 && colDiff == 1) || (rowDiff==1 && colDiff==2))
                    knight.AddAllPossibleMoves(newPos);
            }
        }
        return knight.getAllPossibleMovesCount > 0 ? knight : null;
    }

    private ChessPiece GenerateMovesForQueen(int ind, int thisColCode, int[] board)
    {
        int index = ind;
        int myColorCode = thisColCode;
        int[] chessBoard = board;
        Queen queen = new Queen(myColorCode, index);

        int up = index, down = index;
        int rightDiagIndex = index, leftDiagIndex = index;
        int botLeftD = index, botRightD = index;
        int sideStepRight = index;
        int sideStepLeft = index;
        //UP
        QueenMovement(up , myColorCode ,queen,chessBoard , queen.queenStep);
        //BOTTOM
        QueenMovement(down , myColorCode ,queen,chessBoard, -queen.queenStep);
        QueenMovement(rightDiagIndex , myColorCode ,queen,chessBoard, queen.queenRightDiag);
        QueenMovement(leftDiagIndex , myColorCode ,queen,chessBoard, queen.queenLeftDiag);
        QueenMovement(botLeftD , myColorCode ,queen,chessBoard, queen.queenBotRightDiag);
        QueenMovement(botRightD , myColorCode ,queen,chessBoard, queen.queenBotLeftDiag);
        QueenMovement(sideStepRight , myColorCode ,queen,chessBoard, queen.queenSideStep);
        QueenMovement(sideStepLeft , myColorCode ,queen,chessBoard, -queen.queenSideStep);
        
        
        return queen.getAllPossibleMovesCount > 0 ? queen : null;
    }

    void QueenMovement(int moveDirection, int myColorCode , Queen queen, int[]board , int queenStep)
    {    
        int movePlace = moveDirection;
        movePlace += queenStep;
        
        while (movePlace is >= 0 and  <64 )
        {
            
            int fileSide = movePlace % 8;
            int rankUp = movePlace % 8;
            Console.WriteLine("Stuck in loop");
            int otherColorCode = GetColorCode(board[movePlace]);

            if (board[movePlace] == Piece.Empty) {
                    Console.WriteLine("Stuck in empty loop");
                    queen.AddAllPossibleMoves(movePlace);
                    if ( movePlace>0 && fileSide == 7) break;
                    if ( movePlace>0 && fileSide == 0) break;
                    //next iteration
                    movePlace += queenStep;
            }
            else if (!isSameColorAsMe(myColorCode, otherColorCode))
            {
                Console.WriteLine("Stuck in not same color loop");
                queen.AddAllPossibleMoves(movePlace);
                break;
                //next iteration

            }
            else break;
        }
    }

    private bool isSameColorAsMe(int myColorCode, int otherColorCode)
    {
        return myColorCode == otherColorCode;
    }


    private bool isBlack(int pCode)
    {
        return (pCode & Piece.Black) == Piece.Black;
    }

    public int GetColorCode(int code )
    {
       return isBlack(code) ? Piece.Black : Piece.White;
    }

}

public class Pawn : ChessPiece
{   
    public  readonly int pawnStep = 8;
    public Pawn(int colorCode, int index)
    {
        this.pieceCode =  Piece.Pawn | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
    }
    
}

public class Knight : ChessPiece {
    public int[] knightCanMoveTo = new int[8]
    {
        15,17,10,6,-15,-17,-10,-6
    };
    public Knight(int colorCode , int index)
    {
        this.pieceCode =  Piece.Knight | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
        for (int i = 0; i < 8; i++)
        {
           this.knightCanMoveTo[i] += index;
        }
    }

}

public class Queen : ChessPiece {
    
    public readonly int queenStep = 8 ;
    public readonly int queenRightDiag = 9 ;
    public readonly int queenBotRightDiag = -7 ;
    public readonly int queenLeftDiag = 7 ;
    public readonly int queenBotLeftDiag = -9 ;
    public readonly int queenSideStep = 1 ;
    
    
    public Queen(int colorCode, int index)
    {
        this.pieceCode =  Piece.Queen | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
    }
    
}


public abstract class ChessPiece
{   
    
    protected int pieceCode { get; init; }
    private HashSet<int> allPossibleMovesIndex = new HashSet<int>();
    protected int currentIndex { get; init; }
    
    public int GetPieceCode => pieceCode;
    public int GetCurrentIndex => currentIndex;
    public HashSet<int> GetAllMovesForThisPiece => allPossibleMovesIndex;

     protected bool IsBlack(int colorCode)
    {
        return colorCode == Piece.Black;
    }
     
     public  virtual void AddAllPossibleMoves(int index)
     {       
         allPossibleMovesIndex.Add(index);
     }
     public int getAllPossibleMovesCount => allPossibleMovesIndex.Count;

}

public enum PieceMovementDirection
{
    UP , RIGHT , DOWN , LEFT , RIGHTDIAG ,LEFTDIAG ,RIGHTBOTTOMDIAG,LEFTBOTTOMDIAG 
}
