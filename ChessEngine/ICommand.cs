    using System.Diagnostics;
    using ChessEngine;
    using Newtonsoft.Json;
    using Utility ;
    public interface ICommand
    {
         void Execute();

         void Undo();

         (int, int)? GetInfo();


    }

    
    public abstract class Command : ICommand
    {

        public  MoveType moveType;
        public abstract void Execute();
        public abstract void Undo();

        public abstract (int, int)? GetInfo();
        
        


    }



public class MoveCommand : Command
{

    public new MoveType moveType = MoveType.Regular;
    protected Board savedBoard;
    protected Board currentBoard;
    protected  int currentCell;
    protected int targetCell ;
    protected  int capturedPiece;

    protected ChessEngineSystem engine ;


    //promotion //en Passant


   public MoveCommand(int currnt ,int target , ChessEngineSystem eng  ,Board board )
   {
           
            this.currentCell = currnt ;
            this.targetCell = target ;
            this.engine = eng;
            currentBoard = board;
    }


    //Processed in Engine from UI or the board itself
    public override void Execute()
    {   
        
        this.capturedPiece =currentBoard.chessBoard[targetCell];
        currentBoard.chessBoard[targetCell] = currentBoard.chessBoard[currentCell];
        currentBoard.chessBoard[currentCell] =  Piece.Empty ;
      
      
    }


    public override (int, int)? GetInfo()
    {
        return (currentCell, targetCell);

    }

    //Sent to board for UI .
    public override void Undo()
    {
         currentBoard.chessBoard[currentCell] =  currentBoard.chessBoard[targetCell];
         currentBoard.chessBoard[targetCell] =  capturedPiece;
         engine.UpdateUIWithNewIndex(targetCell , currentCell , capturedPiece);
         this.capturedPiece = Piece.Empty;
         engine.UpdateBoard(currentBoard);
         
    }

}

    
public class CastlingCommand : Command
{
    public new MoveType moveType = MoveType.Castling;
    private Board savedBoard;
    private Board currentBoard;
    private int kingDefaultCell ;
    private int kingNewCell;
    private int RookDefaultCell;
    private int RookNewCell;

    private int color;
    private ChessEngineSystem engine ;



    //When castling is  confirmed , not king or Rook move. Different for them.
    //when king moved -> castling is cancelled anyways .
    //When Rook moved -> it could be king side or Queen side  -> since the king can always 
    //castle to the other remaining one

    public  CastlingCommand ( ChessEngineSystem eng, int KingD , int KingN , int pColor ,  Board  board)
    {
     
        this.savedBoard = new Board(board);
        this.currentBoard = board;
        this.kingDefaultCell = KingD ;
        this.kingNewCell= KingN ;
        this.color =  pColor;
        this.engine = eng ;
      
    }

    public override void Execute()
    {
        
        GameStateManager.Instance.UpdateCastlingCount(); 
        switch(color)
        {
            case  Piece.Black:
                if (kingNewCell > kingDefaultCell)
                {   
                    PerformCastle(-1,63 );
                }
                else 
                    PerformCastle(+1, 56 );
                
                
                
               
                currentBoard.castleRight.blackKingSideCastling = false;       
                currentBoard.castleRight.blackQueenSideCastling = false; 
                Console.ForegroundColor = ConsoleColor.Cyan;
                GameStateManager.Instance.blackCastlingCount++;
              
            
     //    engine.UpdateUIWithNewIndex(RookDefaultCell, RookNewCell);
         break;

            case Piece.White:   
                 if (kingNewCell > kingDefaultCell)  
                     PerformCastle(-1,7);
                 else 
                    PerformCastle(+1,0);              
                 
             
                 currentBoard.castleRight.whiteKingSideCastling = false;       
                 currentBoard.castleRight.whiteQueenSideCastling = false;   
                 Console.ForegroundColor = ConsoleColor.Cyan;
                 GameStateManager.Instance.whiteCastlingCount++;
        // engine.UpdateUIWithNewIndex(RookDefaultCell, RookNewCell);        
        break;
        }
        
    }
    

    public override void Undo()
    {
        engine.UpdateBoard(savedBoard);
        currentBoard.chessBoard[kingDefaultCell]  =   currentBoard.chessBoard[kingNewCell];
        currentBoard.chessBoard[RookDefaultCell] =   currentBoard.chessBoard[RookNewCell];
        currentBoard.chessBoard[kingNewCell] = Piece.Empty;
        currentBoard.chessBoard[RookNewCell] = Piece.Empty;
        engine.UpdateUIWithNewIndex(kingNewCell, kingDefaultCell);
        engine.UpdateUIWithNewIndex(RookNewCell, RookDefaultCell);
        Console.WriteLine("Castling Undo happening now");
     
           
    }

    public override (int, int)? GetInfo()
    {
        return null;
    }


    private void PerformCastle(int step , int rookCell ) // +1 , 0
    {
           // Console.WriteLine($"King new cell is  {kingNewCell}, Rook default cell is at 0 and  { currentBoard.chessBoard[rookCell]}");
            currentBoard.chessBoard[kingNewCell + step] =  currentBoard.chessBoard[rookCell];
            currentBoard.chessBoard[kingNewCell] =   currentBoard.chessBoard[kingDefaultCell];
            currentBoard.chessBoard[kingDefaultCell] = Piece.Empty;
            currentBoard.chessBoard[rookCell] = Piece.Empty;
            RookNewCell = kingNewCell + step;
            this.RookDefaultCell = rookCell;
            Console.ForegroundColor = ConsoleColor.Red;
          //  Console.WriteLine($" CRook new cell is  {RookNewCell}");
          
    }
}



public class RookMoveCommand : Command
{
    public new MoveType moveType = MoveType.Regular;
    int currentCell ;
    int targetCell;
    
    int targetCheck ;
    int pColor ;

    int capturedPiece;
    public Board savedBoard;

    bool BkingSideRook;
    bool BQueenSideRook;

    bool WKingSideRook;
    bool WQueenSideRook;
    
    private Board currentBoard;
    
    private ChessEngineSystem engine;

    public RookMoveCommand (ChessEngineSystem engs,int old , int newCell ,int color ,  Board board )
    {
        savedBoard = new Board(board, "Clone from rook move");
        currentBoard = board;
        this.engine = engs;
        this.currentCell = old ;
        this.targetCell =  newCell;
        this.pColor = color;
        this.targetCheck = old % 8 ; //file 0 or 7
    }

    public override void Execute()
    { 
       switch(pColor)
       {
        case Piece.White:
            if (targetCheck == 0 && currentBoard.castleRight.whiteQueenSideCastling)
            {   
               
                currentBoard.castleRight.whiteQueenSideCastling = false;
            }
            else if(targetCheck == 7&& currentBoard.castleRight.whiteKingSideCastling)
                currentBoard.castleRight.whiteKingSideCastling = false;
            break;   
         
        case Piece.Black:
            if (targetCheck  == 0 && currentBoard.castleRight.blackQueenSideCastling)
                currentBoard.castleRight.blackQueenSideCastling = false;
            else if (targetCheck == 7 && currentBoard.castleRight.blackKingSideCastling)
                currentBoard.castleRight.blackKingSideCastling = false;
            break;
       }
       
       capturedPiece =   currentBoard.chessBoard[targetCell];   
       currentBoard.chessBoard[targetCell] =   currentBoard.chessBoard[currentCell];
       currentBoard.chessBoard[currentCell] = Piece.Empty;
       
       
    }

    public override void Undo()
    {
        
        currentBoard.chessBoard[currentCell] = currentBoard.chessBoard[targetCell];
        currentBoard.chessBoard[targetCell] = capturedPiece;
   

    engine.UpdateUIWithNewIndex(targetCell, currentCell,capturedPiece );
    capturedPiece = Piece.Empty;
    engine.UpdateBoard(savedBoard);

    }

    public override (int, int)? GetInfo()
    {
        return null;
    }
}

public class KingMoveCommand : MoveCommand
{   
    int pColor;
    
    public KingMoveCommand(int currnt, int target, ChessEngineSystem eng , int color ,  Board board) : base(currnt, target, eng, board)
    {   
        pColor = color;
        savedBoard = new Board(board, "cloned from King move");
        currentBoard = board;

    }

    public override void Execute()
    {
        
        switch(pColor)
        {
            case Piece.White:
               
                currentBoard.castleRight.whiteKingSideCastling = false;
                currentBoard.castleRight.whiteQueenSideCastling = false;
                break;
            case Piece.Black:
                currentBoard.castleRight.blackKingSideCastling = false;
                currentBoard.castleRight.blackQueenSideCastling = false;
                break;    
        }

        base.Execute();

    }

    public override void Undo()
    {
        base.Undo();
        

    }

}
    
    public class EnPassantCommand : MoveCommand
    {
        public new MoveType moveType = MoveType.EnPassant;
        private int capturedPawnIndex;
        
        public EnPassantCommand(int currnt, int target, ChessEngineSystem eng ,int capPawnIndex , Board board) : base(currnt, target, eng , board)
        {
            capturedPawnIndex = capPawnIndex;
            // = eng.Get//;
        }


        public override void Execute()
        {
            this.capturedPiece =  currentBoard.chessBoard[capturedPawnIndex];
            currentBoard.chessBoard[targetCell] =  currentBoard.chessBoard[currentCell];
            currentBoard.chessBoard[currentCell] =  Piece.Empty ;
            currentBoard.chessBoard[capturedPawnIndex] =  Piece.Empty ; 
            
           // Console.WriteLine($"en Passant executed , caching the captured index -> {capturedPawnIndex}");
            engine.UpdateUIWithNewIndex(capturedPawnIndex);
          
        }

        public override void Undo()
        {
            currentBoard.chessBoard[currentCell] = currentBoard.chessBoard[targetCell];
            currentBoard.chessBoard[capturedPawnIndex] = capturedPiece;
            currentBoard.chessBoard[targetCell] = Piece.Empty; //only because it is En Passant.
            engine.UpdateUIWithNewIndex(targetCell , currentCell , capturedPiece, capturedPawnIndex);
            this.capturedPiece = Piece.Empty;
            engine.UpdateBoard(currentBoard);
       
           

        }
    }
    public class PromotionCommand : MoveCommand
    {

        public new MoveType moveType = MoveType.Promotion;
        public int promotedToPiece = -1;
        private ChessPiece piece;
        public PromotionCommand(int currnt, int target, int PromoPiece, ChessPiece p,ChessEngineSystem eng ,  Board board) : base(currnt, target, eng , board)
        {
            this.promotedToPiece = PromoPiece;
            capturedPiece =  currentBoard.chessBoard[targetCell];
            piece = p;
            var pwn = (Pawn)piece;
            pwn.pormotionStatus = true;
            
        }

        public override void Execute()
        {
            currentBoard.chessBoard[targetCell] = promotedToPiece;
            currentBoard.chessBoard[currentCell] = Piece.Empty;
            engine.UpdateUIWithNewIndex(targetCell , currentCell , promotedToPiece);

        }

        public override void Undo()
        {
            currentBoard.chessBoard[currentCell] = piece.GetPieceCode;
            currentBoard.chessBoard[targetCell] = capturedPiece;
            promotedToPiece = -1;
           // engine.UpdateUIWithNewIndex(targetCell ,  );
            var pwn = (Pawn)piece;
          //  pwn.promotionPieces.Push(promotedToPiece);
            pwn.pormotionStatus = false;
            //Update in UI as well
          //  engine.GetBoardClass.ShowBoard();
        engine.UpdateBoard(currentBoard);
        }
    }
