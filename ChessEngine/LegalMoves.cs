
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Utility;

namespace ChessEngine;
// This is a set in stone , defined file . 

sealed class LegalMoves
{
    private Stopwatch watch = new Stopwatch();
    // need to run like a time check on how long this takes to run and-
    // further optimize it
    public List<ChessPiece>? GenerateLegalMoves(Board b, int colorToMove , bool isCustom)
    {
        int len = b.chessBoard.Length;
        var board = b.chessBoard;

        List<ChessPiece> moveList = new List<ChessPiece>(); // it has already been worked on
        for (int i = 0; i < len; i++)
        {
            if (board[i] == Piece.Empty)
                continue;

            int pieceCode = board[i] & Piece.CPiece;
            int colorCode = GetColorCode(board[i]);
            if (colorCode != colorToMove)
                continue;

            switch (pieceCode)
            {
                case Piece.Pawn:
                {
                    ChessPiece p = GenerateMovesForPawn(i, colorCode, b);
                    if (p != null) moveList.Add(p);
                    break;
                }
                case Piece.Knight:
                {
                    ChessPiece p = GenerateMovesForKnight(i, colorCode, b);
                    if (p != null) moveList.Add(p);
                    break;
                }
                case Piece.Queen:
                {
                    ChessPiece p = GenerateMovesForQueen(i, colorCode, b);
                    if (p != null) moveList.Add(p);
                    break;
                }
                case Piece.Bishop:
                {
                    ChessPiece p = GenerateMovesForBishop(i, colorCode, b);
                    if (p != null) moveList.Add(p);
                    break;
                }
                case Piece.Rook:
                {
                    ChessPiece p = GenerateMovesForRook(i, colorCode, b);
                    if (p != null) moveList.Add(p);
                    break;
                }
                case Piece.King:
                {
                    ChessPiece p = GenerateMovesForKing(i, colorCode, b , isCustom);
                    if (p != null) moveList.Add(p);
                    break;
                }
            }
        }

        return moveList;
        //This wraps up all the moves, we need the FINISHED ALL PIECES THAT CAN MAKE MOVE

    }

    //going thru every  piece
    private ChessPiece GenerateMovesForPawn(int ind,int colCode, Board board)
    {
            int index = ind;
            int thisColorCode = colCode;
            Pawn pawn = new Pawn(thisColorCode, ind);

            int stepBasedOnColour = colCode == Piece.White ? pawn.pawnStep : -pawn.pawnStep;
            var ApplyIndexBasedOnColor = colCode == Piece.White ? index + stepBasedOnColour : index + stepBasedOnColour;

            if (ApplyIndexBasedOnColor is < 0 or > 63) return null;
            

            //front move -> normal move
            if ( board.chessBoard[ApplyIndexBasedOnColor] == Piece.Empty)
            {
                pawn.AddAllPossibleMoves(ApplyIndexBasedOnColor);

                if (ChessEngineSystem.Instance.IsPawnDefIndex(colCode, ind))
                {   
                  
                    if ((ApplyIndexBasedOnColor + stepBasedOnColour is >= 0 and < 64 ) && board.chessBoard[ApplyIndexBasedOnColor + stepBasedOnColour] == Piece.Empty)
                    {
                            pawn.AddAllPossibleMoves(ApplyIndexBasedOnColor + stepBasedOnColour);
                        
                    }
                }
            }

            int targetRow =
                (ApplyIndexBasedOnColor) / 8; // check if all moves are in same row ,front and front sides (Diagonals)
            //RIGHT MOVE
            int rdIndex = ApplyIndexBasedOnColor + 1;

            if (rdIndex is >= 0 and < 64)
            {
                int RDcolorCode = isBlack(board.chessBoard[rdIndex]) ? Piece.Black : Piece.White;
                if (rdIndex / 8 == targetRow && thisColorCode != RDcolorCode && board.chessBoard[rdIndex] != Piece.Empty)
                    pawn.AddAllPossibleMoves(rdIndex);
            }


            int ldIndex = ApplyIndexBasedOnColor - 1;
            if (ldIndex is >= 0 and < 64)
            {
                int LDcolorCode = isBlack(board.chessBoard[ldIndex]) ? Piece.Black : Piece.White;
                if (ldIndex / 8 == targetRow && thisColorCode != LDcolorCode && board.chessBoard[ldIndex] != Piece.Empty)
                    pawn.AddAllPossibleMoves(ldIndex);
            }


            PawnCheckEnPassant(pawn, index, colCode, board.chessBoard);


            //Prevent looping all times
            return pawn.GetAllMovesForThisPiece.Count > 0 ? pawn : null;
        
       
    }
    private void   PawnCheckEnPassant(Pawn pawn , int index,int colCode ,int[] board)
    {
        int cellFinal, cellOld;
       

        if (ChessEngineSystem.Instance.moveHistory.Count == 0)
            return;
        
       var piece =  ChessEngineSystem.Instance.moveHistory.Peek().GetInfo();
       if (piece == null) return;


       cellFinal = piece.Value.Item2;
       cellOld = piece.Value.Item1;
     
       if (Math.Abs(cellFinal - cellOld) == 16 && // The last move was a double move
           ( board[cellFinal] & Piece.CPiece)  == Piece.Pawn  && 
           !IsSameColorAsMe(colCode, GetColorCode( board[cellFinal]))
           )
       {
           if (Math.Abs(cellFinal % 8 - index % 8) == 1 && (cellFinal / 8 == index /8))
           {
               int enPassantIndex = colCode == Piece.White ? cellFinal + 8 : cellFinal - 8;
        //       Console.WriteLine($"Found En Passant move for at {enPassantIndex} ");
               
               //we also need cellFinal
               pawn.specialIndex = enPassantIndex; //Success
               pawn.AddAllPossibleMoves(enPassantIndex);
           }
       }
       
//SEPERATE EN PASSANT LIST FOR UNDOING AND A SPECIAL ORDER , track of en passant index
    }

    private ChessPiece GenerateMovesForKnight(int ind, int thisColCode, Board board)
    {
        int index = ind;
        int myColCode = thisColCode;
        Knight knight = new Knight(myColCode, index);
        int movesLength = knight.knightCanMoveTo.Length;
        for (int i = 0; i < movesLength; i++)
        {   
            if( knight.knightCanMoveTo[i] <= 0 || knight.knightCanMoveTo[i] > 63)
                continue;
            
            int otherPColorCode = GetColorCode(board.chessBoard[knight.knightCanMoveTo[i]]);
            bool isSameColor = IsSameColorAsMe(myColCode, otherPColorCode);
               
    
            if (board.chessBoard[knight.knightCanMoveTo[i]] == Piece.Empty)
            {
                
                int newPos =knight.knightCanMoveTo[i];
                //After having all moves  , we try to check for L move shape 
                //INTERESTING LOGIC FOR THIS -> coz it always moves 2 spaces in 1 dir and 1 dir perpendicular
                int rowDiff = (int)MathF.Abs(newPos / 8 - ind / 8);
                int colDiff = (int)MathF.Abs(newPos % 8 - ind % 8);
                
                if((rowDiff ==2 && colDiff == 1) || (rowDiff==1 && colDiff==2))
                    knight.AddAllPossibleMoves(newPos);
            }
            
            if (!isSameColor)
            {
                int newPos =knight.knightCanMoveTo[i];
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
    
    private ChessPiece GenerateMovesForQueen(int ind, int thisColCode, Board board)
    {
        int currentIndex = ind;
        int myColorCode = thisColCode;
        int[] chessBoard = board.chessBoard;
        Queen queen = new Queen(myColorCode, currentIndex);
        
        GenerateAllMoves(currentIndex , myColorCode ,queen,chessBoard);
        return queen.getAllPossibleMovesCount > 0 ? queen : null;
    }
    
    private ChessPiece GenerateMovesForBishop(int ind, int thisColCode,Board board)
    {    int currentIndex = ind;
        int myColorCode = thisColCode;
        int[] chessBoard = board.chessBoard;
        Bishop bishop = new Bishop(myColorCode, currentIndex);
        
        GenerateAllMoves(currentIndex, myColorCode, bishop, chessBoard);
        return bishop.getAllPossibleMovesCount>0? bishop: null;
    }
    
    private ChessPiece GenerateMovesForRook(int ind, int thisColCode,Board board)
    {   
        int currentIndex = ind;
        int myColorCode = thisColCode;
        int[] chessBoard = board.chessBoard;
        Rook rook = new Rook(myColorCode, currentIndex);
        
        GenerateAllMoves(currentIndex , myColorCode , rook, chessBoard);
        return rook.getAllPossibleMovesCount>0? rook: null;
    }

    private ChessPiece GenerateMovesForKing(int ind, int thisColCode, Board board , bool isCustom)
    {
        int index = ind;
        int myColCode = thisColCode;
        King king = new King(myColCode, index);
        int movesLength = king.kingCanMoveTo.Length;
        for (int i = 0; i < movesLength; i++)
        { 
            if (king.kingCanMoveTo[i] < 0 || king.kingCanMoveTo[i] > 63)
            {   
               
                continue;
            }
            
            if (king.GetCurrentIndex % 8 == 7 &&  king.kingCanMoveTo[i] % 8 == 0)
            {
                continue;
            }

            if (king.GetCurrentIndex % 8 == 0 &&  king.kingCanMoveTo[i] % 8 == 7)
            {   
                continue;
            }


            int otherPColorCode = GetColorCode(board.chessBoard[king.kingCanMoveTo[i]]);
            bool isSameColor = IsSameColorAsMe(myColCode, otherPColorCode);
    
            if (board.chessBoard[king.kingCanMoveTo[i]] == Piece.Empty)
            {
                king.AddAllPossibleMoves(king.kingCanMoveTo[i]);
                
            }
            else if (!isSameColor) king.AddAllPossibleMoves(king.kingCanMoveTo[i]);
        }
        // here we can generate castling content
        //

        if (index is < 0 or >= 64) return king.getAllPossibleMovesCount > 0 ? king : null;

      //  if (isCustom) return king.getAllPossibleMovesCount > 0 ? king : null; // since castling cant result in our getting checked
        
        
        //revamp this please !!
        switch (thisColCode)
        {
            case Piece.Black when GameStateManager.Instance.isBlackCastlingAvailable &&
                                  !GameStateManager.Instance.blackKingInCheck:
            {
                if (!GameStateManager.Instance.blackKingSideRookMoved)
                {
                    CastlingKingSideCompute(index, board, king, +2, +1, Piece.White);
                }

                if (!GameStateManager.Instance.blackQueenSideRookMoved)
                {
                    CastlingKingQueenSideCompute(index,  board, king, -2, -1, Piece.White);
                }

                break;
            }
            case Piece.White when GameStateManager.Instance.isWhiteCastlingAvailable &&
                                  !GameStateManager.Instance.whiteKingInCheck:
            {
                if (!GameStateManager.Instance.whiteKingSideRookMoved)
                {
                    CastlingKingSideCompute(index, board, king, +2, +1, Piece.Black);
                }

                if (!GameStateManager.Instance.whiteQueenSideRookMoved)
                {
                    CastlingKingQueenSideCompute(index, board, king, -2, -1, Piece.Black);
                }

                break;
            }
        }

        return king.getAllPossibleMovesCount > 0 ? king : null;
    }

    private void  CastlingKingSideCompute(int index,Board board , ChessPiece king , int maxStep ,int minStep, int turnToCheck )
    {
        for (int i =index +minStep; i <=  index + maxStep; i++)
        {
            if (board.chessBoard[i] != Piece.Empty) break;
            
          
            var oppMoveList =  board. GenerateMoves(turnToCheck , board, true);
            foreach (var piece in oppMoveList )
            {
                if (piece.GetAllMovesForThisPiece.Contains(i)) return;
                king.AddAllPossibleMoves(i);
                
            }
        }
    }
    private void  CastlingKingQueenSideCompute(int index,Board board , ChessPiece king , int maxStep ,int minStep , int turnToCheck )
    {
        for (int i =index +minStep; i >=  index + maxStep-1; i--)
        {
          
            if (board.chessBoard[i] != Piece.Empty) break;
            
            if(board.chessBoard[i] != index+ maxStep-1)
                continue;
         
            //Scan for opponent moves - in Advance
            var oppMoveList = board.GenerateMoves( turnToCheck , board , true );
            foreach (var piece in oppMoveList )
            {
                if (piece.GetAllMovesForThisPiece.Contains(i)) return;
                king.AddAllPossibleMoves(i);
                
            }
        }
    }

    #region MovementLogic For Queen , Rook , Bishop 
        
        private void GenerateAllMoves(int currentIndex, int myColorCode , ChessPiece thisPiece, int[]board )
        {
            foreach (PieceMovementDirection direction in thisPiece.GetMovDirectionForThis)
            {
                switch (direction)
                {
                    case PieceMovementDirection.Up:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.Up);
                        break;
                    case PieceMovementDirection.Down:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.Down);
                        break;
                    case PieceMovementDirection.Right:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.Right);
                        break;
                    case PieceMovementDirection.Left:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.Left);
                        break;
                    case PieceMovementDirection.RightDiag:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.RightDiag);
                        break;
                    case PieceMovementDirection.LeftDiag:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.LeftDiag);
                        break;
                    case PieceMovementDirection.RightBotDiag:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.RightBotDiag);
                        break;
                    case PieceMovementDirection.LeftBotDiag:
                        DirectionMovement( currentIndex,  myColorCode , thisPiece, board , PieceMovementDirection.LeftBotDiag);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
            
        }

        private void DirectionMovement(int currentIndex, int myColorCode , ChessPiece thisPiece, int[]board ,PieceMovementDirection stepsDirAndMove)
        {
            int stepsICanMove = (int)stepsDirAndMove;
            int myNewPosition = currentIndex;
            myNewPosition  = myNewPosition + stepsICanMove;
            
            while (myNewPosition >=0 && myNewPosition < 64)
            {
                switch (stepsDirAndMove)
                {
                    case PieceMovementDirection.Up when (currentIndex / 8 == 7):
                    case PieceMovementDirection.Down when (currentIndex / 8 == 0):
                    case PieceMovementDirection.Left when (currentIndex % 8 == 0):
                    case PieceMovementDirection.Right when (currentIndex % 8 == 7):
                    case PieceMovementDirection.RightDiag when (currentIndex % 8 == 7):
                    case PieceMovementDirection.LeftDiag when (currentIndex % 8 == 0):
                    case PieceMovementDirection.LeftBotDiag when (currentIndex % 8 == 0):
                    case PieceMovementDirection.RightBotDiag when (currentIndex % 8 == 7):
                        return;
                }
                 int otherPColorCode = GetColorCode(board[myNewPosition]);
                 if (IsSameColorAsMe(myColorCode, otherPColorCode))
                     break;
                 if (board[myNewPosition] == Piece.Empty)
                 { 
                      thisPiece.AddAllPossibleMoves(myNewPosition);
                      if (stepsDirAndMove == PieceMovementDirection.Up && (myNewPosition / 8 == 7))    break;
                      if (stepsDirAndMove == PieceMovementDirection.Down && (myNewPosition / 8 == 0))   break;
                      if (stepsDirAndMove == PieceMovementDirection.Left && (myNewPosition % 8 == 0))   break;
                      if (stepsDirAndMove == PieceMovementDirection.Right && (myNewPosition % 8 == 7))  break;
                      if (stepsDirAndMove == PieceMovementDirection.RightDiag && (myNewPosition % 8 == 7)) break;
                      if (stepsDirAndMove == PieceMovementDirection.LeftDiag && (myNewPosition % 8 == 0)) break;
                      if (stepsDirAndMove == PieceMovementDirection.LeftBotDiag && (myNewPosition % 8 == 0)) break;
                      if (stepsDirAndMove == PieceMovementDirection.RightBotDiag && (myNewPosition % 8 == 7) ) break;
                      myNewPosition += stepsICanMove;
                     continue;
                 }
                 
                 //Encountered an Opponent piece
                thisPiece.AddAllPossibleMoves(myNewPosition);
                 break;
            }
        }

    #endregion
    
    private bool IsSameColorAsMe(int myColorCode, int otherColorCode)
    {
        return myColorCode == otherColorCode;
    }
    
    private bool isBlack(int pCode)
    {
        return (pCode & Piece.Black) == Piece.Black;
    }

    private int GetColorCode(int code )
    {
        if (isBlack(code))
            return Piece.Black;
        
        return (code & Piece.White) == Piece.White ? Piece.White : Piece.Empty;
    }


 

}

public class Pawn : ChessPiece
{   
    public  readonly int pawnStep = 8;
    public bool pormotionStatus = false;
    
    public Pawn(int colorCode, int index)
    {   
        this.pieceCode =  Piece.Pawn | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
      
    }

    public List<int> enPassantMoveList;

}

[SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
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


public class King : ChessPiece
{
    public int KingCastlingUnits = 2;
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
    
    public Queen(int colorCode, int index)
    {
        this.pieceCode =  Piece.Queen | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
        possibleDirection = new List<PieceMovementDirection>()
            {
                PieceMovementDirection.Up , PieceMovementDirection.Down ,
                PieceMovementDirection.Left , PieceMovementDirection.Right ,
                PieceMovementDirection.RightDiag , PieceMovementDirection.LeftDiag ,
                PieceMovementDirection.RightBotDiag , PieceMovementDirection.LeftBotDiag
            };
    }
    
}


public class Bishop : ChessPiece
{
    public Bishop(int colorCode, int index)
    {   
       
        this.pieceCode =  Piece.Bishop | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
        possibleDirection = new List<PieceMovementDirection>()
        {
            PieceMovementDirection.RightDiag , PieceMovementDirection.LeftDiag ,
            PieceMovementDirection.RightBotDiag , PieceMovementDirection.LeftBotDiag
        };
    }
}

public class Rook : ChessPiece
{
    
    public Rook(int colorCode, int index)
    {
        this.pieceCode =  Piece.Rook | ( IsBlack(colorCode) ? Piece.Black : Piece.White);
        this.currentIndex = index;
        possibleDirection = new List<PieceMovementDirection>()
        {
            PieceMovementDirection.Up, PieceMovementDirection.Down,
            PieceMovementDirection.Right, PieceMovementDirection.Left
        };
    }
}


public abstract class ChessPiece
{   
    
    protected int pieceCode { get; init; }
    public HashSet<int> allPossibleMovesIndex = new HashSet<int>();
    protected int currentIndex { get; init; }
    public int specialIndex = 999;

    protected List<PieceMovementDirection> possibleDirection;
    
    public int GetPieceCode => pieceCode;
    public int GetCurrentIndex => currentIndex;
    public HashSet<int> GetAllMovesForThisPiece => allPossibleMovesIndex;

     protected bool IsBlack(int colorCode)
    {
        return colorCode == Piece.Black;
    }

     public List<PieceMovementDirection> GetMovDirectionForThis => possibleDirection;
     
     public  virtual void AddAllPossibleMoves(int index)
     {       
         allPossibleMovesIndex.Add(index);
     }
     public int getAllPossibleMovesCount => allPossibleMovesIndex.Count;

} 
public enum PieceMovementDirection
{
    Up  = 8,Down = -8  , Right =1  , Left = -1, RightDiag= 9 , LeftDiag = 7, RightBotDiag = -7 ,LeftBotDiag = -9
}



