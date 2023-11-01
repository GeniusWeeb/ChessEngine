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
            if (pieceCode == Piece.Bishop)
            {
                ChessPiece p = GenerateMovesForBishop(i , colorCode,board);
                if(p!=null)  GameStateManager.Instance.allPiecesThatCanMove.Add(p);
            } 
            if (pieceCode == Piece.Rook)
            {
                ChessPiece p = GenerateMovesForRook(i , colorCode,board);
                if(p!=null)  GameStateManager.Instance.allPiecesThatCanMove.Add(p);
            }
            if (pieceCode == Piece.King)
            {
                ChessPiece p = GenerateMovesForKing(i , colorCode,board);
                if(p!=null)  GameStateManager.Instance.allPiecesThatCanMove.Add(p);
            
            }
            

        }
        watch.Stop();
        //Console.WriteLine( $"Time it take to iterate thru the board{watch.Elapsed}");
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
    
    
    private ChessPiece GenerateMovesForKing(int ind, int thisColCode, int[] board)
    {
        int index = ind;
        int mycolCode = thisColCode;
        King king = new King(mycolCode, index);
        int movesLength = king.kingCanMoveTo.Length;
        for (int i = 0; i < movesLength; i++)
        {   
            if( king.kingCanMoveTo[i] <= 0 || king.kingCanMoveTo[i] > 63)
                continue;
            
            int otherColorCode = GetColorCode(king.kingCanMoveTo[i]);
            bool sameColor = isSameColorAsMe(mycolCode, otherColorCode);

            if (board[king.kingCanMoveTo[i]] == Piece.Empty)
            {
                king.AddAllPossibleMoves(king.kingCanMoveTo[i]);
                
            }
            else if (!sameColor)   king.AddAllPossibleMoves(king.kingCanMoveTo[i]);
            
        }
        return king.getAllPossibleMovesCount > 0 ? king : null;
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
        GenerateAllMoves(up , myColorCode ,queen,chessBoard , queen.queenStep);
        //BOTTOM
        GenerateAllMoves(down , myColorCode ,queen,chessBoard, -queen.queenStep);
        GenerateAllMoves(rightDiagIndex , myColorCode ,queen,chessBoard, queen.queenRightDiag);
        GenerateAllMoves(leftDiagIndex , myColorCode ,queen,chessBoard, queen.queenLeftDiag);
        GenerateAllMoves(botLeftD , myColorCode ,queen,chessBoard, queen.queenBotRightDiag);
        GenerateAllMoves(botRightD , myColorCode ,queen,chessBoard, queen.queenBotLeftDiag);
        GenerateAllMoves(sideStepRight , myColorCode ,queen,chessBoard, queen.queenSideStep);
        GenerateAllMoves(sideStepLeft , myColorCode ,queen,chessBoard, -queen.queenSideStep);
        
        
        return queen.getAllPossibleMovesCount > 0 ? queen : null;
    }
    
    private ChessPiece GenerateMovesForBishop(int ind, int thisColCode,int[] board)
    {    int index = ind;
        int myColorCode = thisColCode;
        int[] chessBoard = board;
        Bishop bishop = new Bishop(myColorCode, index);


        int LD = index , RD = index , leftBotDiag = index , rightBotDiag = index;
        
        GenerateAllMoves(LD , myColorCode , bishop, chessBoard , bishop.leftDiagonal);
        GenerateAllMoves(RD , myColorCode , bishop, chessBoard , bishop.rightDiagonal);
        GenerateAllMoves(leftBotDiag, myColorCode , bishop, chessBoard , bishop.bottomLeftDiagonal);
        GenerateAllMoves(rightBotDiag, myColorCode , bishop, chessBoard , bishop.bottomRightDiagonal);

        return bishop.getAllPossibleMovesCount>0? bishop: null;
    }
    
    private ChessPiece GenerateMovesForRook(int ind, int thisColCode,int[] board)
    {    int index = ind;
        int myColorCode = thisColCode;
        int[] chessBoard = board;
        Rook rook = new Rook(myColorCode, index);


        int up = index , down = index , left = index , right = index;
        
        GenerateAllMoves(up , myColorCode , rook, chessBoard , rook.up);
        GenerateAllMoves(down , myColorCode , rook, chessBoard , rook.down);
        GenerateAllMoves(left, myColorCode , rook, chessBoard , rook.left);
        GenerateAllMoves(right, myColorCode , rook, chessBoard , rook.right);

        return rook.getAllPossibleMovesCount>0? rook: null;
    }

    private void GenerateAllMoves(int moveDirection, int myColorCode , ChessPiece mainPiece, int[]board , int queenStep)
    {    
        int movePlace = moveDirection;
        movePlace += queenStep;
        
        while (movePlace is >= 0 and  <64 )
        {
            
            int fileSide = movePlace % 8;
            int rankUp = movePlace % 8;
            int otherColorCode = GetColorCode(board[movePlace]);
            if (board[movePlace] == Piece.Empty) {
                
                    mainPiece.AddAllPossibleMoves(movePlace);
                    if ( movePlace>0 && fileSide == 7) break;
                    if ( movePlace>0 && fileSide == 0) break;
                    //next iteration
                    movePlace += queenStep;
            }
            else if (!isSameColorAsMe(myColorCode, otherColorCode))
            {
               
                mainPiece.AddAllPossibleMoves(movePlace);
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


public class King : ChessPiece { 
    public int[] kingCanMoveTo = new int[8]
    {
       7 ,8,9 ,-1,+1 ,-9,-8,-7
    };
    public King(int colorCode , int index)
    {
        this.pieceCode =  Piece.King | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
        for (int i = 0; i < 8; i++)
        {
           this.kingCanMoveTo[i] += index;
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


public class Bishop : ChessPiece
{
    public readonly int rightDiagonal = 9;
    public readonly int leftDiagonal =  7;
    public readonly int bottomLeftDiagonal = -9;
    public readonly int bottomRightDiagonal = -7;
    
    public Bishop(int colorCode, int index)
    {
        this.pieceCode =  Piece.Bishop | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
    }
}

public class Rook : ChessPiece
{
    public readonly int up = +8;
    public readonly int down =  -8;
    public readonly int left = -1;
    public readonly int right = +1;
    
    public Rook(int colorCode, int index)
    {
        this.pieceCode =  Piece.Rook | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
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
