    using ChessEngine;
    using Utility ;
    public interface ICommand
    {
         void Execute();

         void Undo();

         (int, int)? GetInfo();


    }

    
    public abstract class Command : ICommand 
    {   
        
        public abstract void Execute();
        public abstract void Undo();

        public abstract (int, int)? GetInfo();


    }



public class MoveCommand : Command
{



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
        currentBoard.ShowBoard();
        
        Console.WriteLine("Move added to board");
      
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
        this.currentBoard = board;
        this.kingDefaultCell = KingD ;
        this.kingNewCell= KingN ;
        this.color =  pColor;
        this.engine = eng ;
      
    }

    public override void Execute()
    {
        switch(color)
        {
          
            case  Piece.Black:  
                 if (kingNewCell > kingDefaultCell)  
                        PerformCastle(-1,63);
                 else 
                        PerformCastle(+1, 56);      
                     
         Console.WriteLine("Black Castling is confirmed");
                 GameStateManager.Instance.castlingCount += 1;        
       
         engine.UpdateUIWithNewIndex(RookDefaultCell, RookNewCell);
         break;

            case Piece.White:   
                 if (kingNewCell > kingDefaultCell)  
                     PerformCastle(-1,7);
                 else 
                    PerformCastle(+1,0);      
                     
         Console.WriteLine("White Castling is confirmed"); 
                 GameStateManager.Instance.castlingCount += 1; 
       
         engine.UpdateUIWithNewIndex(RookDefaultCell, RookNewCell);        
        break;
        }
    }
    

    public override void Undo()
    {
        switch(color)
        {
            case Piece.Black:
            GameStateManager.Instance.isBlackCastlingAvailable = true;
            break;
            case Piece.White:
            GameStateManager.Instance.isWhiteCastlingAvailable = true;
            break;
        }
        currentBoard.chessBoard[kingDefaultCell]  =   currentBoard.chessBoard[kingNewCell];
        currentBoard.chessBoard[RookDefaultCell] =   currentBoard.chessBoard[RookNewCell];
        currentBoard.chessBoard[kingNewCell] = Piece.Empty;
        currentBoard.chessBoard[RookNewCell] = Piece.Empty;
        engine.UpdateUIWithNewIndex(kingNewCell, kingDefaultCell);
        engine.UpdateUIWithNewIndex(RookNewCell, RookDefaultCell);
        Console.WriteLine("Castling Undo happening now");
        engine.UpdateBoard(currentBoard);
           
    }

    public override (int, int)? GetInfo()
    {
        return null;
    }


    private void PerformCastle(int step , int rookCell ) // +1 , 0
    {
            Console.WriteLine($"King new cell is  {kingNewCell}, Rook default cell is at 0 and  { currentBoard.chessBoard[rookCell]}");
            currentBoard.chessBoard[kingNewCell + step] =  currentBoard.chessBoard[rookCell];
            currentBoard.chessBoard[kingNewCell] =   currentBoard.chessBoard[kingDefaultCell];
            currentBoard.chessBoard[kingDefaultCell] = Piece.Empty;
            currentBoard.chessBoard[rookCell] = Piece.Empty;
            RookNewCell = kingNewCell + step;
            this.RookDefaultCell = rookCell;
            Console.WriteLine($" CRook new cell is  {RookNewCell}");
          
    }
}



public class RookMoveCommand : Command
{
    
    int currentCell ;
    int targetCell;
    
    int targetCheck ;
    int pColor ;

    int capturedPiece;

    bool BkingSideRook;
    bool BQueenSideRook;

    bool WKingSideRook;
    bool WQueenSideRook;

    private Board currentBoard;
    
    private ChessEngineSystem engine;

    public RookMoveCommand (ChessEngineSystem engs,int old , int newCell ,int color ,  Board board )
    {
        currentBoard = board;
        this.engine = engs;
        this.currentCell = old ;
        this.targetCell =  newCell;
        this.pColor = color;
        this.targetCheck = old % 8 ; //file 0 or 7
       
        
      
    }

    public override void Execute()
    {   

      capturedPiece =   currentBoard.chessBoard[targetCell];   
      currentBoard.chessBoard[targetCell] =   currentBoard.chessBoard[currentCell];
      currentBoard.chessBoard[currentCell] = Piece.Empty;
        
      if(!GameStateManager.Instance.isWhiteCastlingAvailable && !GameStateManager.Instance.isBlackCastlingAvailable)
        return;

       switch(pColor)
       {
        case Piece.White:
            if(targetCheck == 7 && !GameStateManager.Instance.whiteKingSideRookMoved)
                this.WKingSideRook=  GameStateManager.Instance.whiteKingSideRookMoved = true ;
            else if( targetCheck == 0 && !GameStateManager.Instance.whiteQueenSideRookMoved)
                this. WQueenSideRook= GameStateManager.Instance.whiteQueenSideRookMoved = true;    
        break;
        case Piece.Black:
            if(targetCheck == 7 && !GameStateManager.Instance.blackKingSideRookMoved)
               this.BkingSideRook = GameStateManager.Instance.blackKingSideRookMoved = true;
            else  if( targetCheck == 0 && !GameStateManager.Instance.blackQueenSideRookMoved)
               this.BQueenSideRook = GameStateManager.Instance.blackQueenSideRookMoved = true;    
           
        break;
       }
    }

    public override void Undo()
    {
        switch(pColor)
        {
            case Piece.White:
                if(targetCheck == 7 && WKingSideRook)
                    GameStateManager.Instance.whiteKingSideRookMoved = false;
                else if (targetCheck == 0 && WQueenSideRook)
                    GameStateManager.Instance.whiteQueenSideRookMoved = false;    
                break;
            case Piece.Black:
                if(targetCheck == 7 && BkingSideRook)
                    GameStateManager.Instance.blackKingSideRookMoved = false;
                else if (targetCheck == 0 && BQueenSideRook)
                    GameStateManager.Instance.blackQueenSideRookMoved = false;    
                break;    
        }

        currentBoard.chessBoard[currentCell] = currentBoard.chessBoard[targetCell];
        currentBoard.chessBoard[targetCell] = capturedPiece;
   

    engine.UpdateUIWithNewIndex(targetCell, currentCell,capturedPiece );
    capturedPiece = Piece.Empty;
    engine.UpdateBoard(currentBoard);

    }

    public override (int, int)? GetInfo()
    {
        return null;
    }
}

public class kingMoveCommand : MoveCommand
{   
    
    int pColor;
    
    public kingMoveCommand(int currnt, int target, ChessEngineSystem eng , int color ,  Board board) : base(currnt, target, eng, board)
    {
        pColor = color;
        // = eng.Get//;
    }

    public override void Execute()
    {
        currentBoard.ShowBoard();
        Console.WriteLine($"King moved from {currentCell} to {targetCell}");
        switch(pColor)
        {
            case Piece.White:
                if(GameStateManager.Instance.isWhiteCastlingAvailable)
                   Console.WriteLine("White King castle");
                break;
            case Piece.Black:
                 if(GameStateManager.Instance.isBlackCastlingAvailable)    
                  Console.WriteLine("Black king castle");
                break;    
        }

      base.Execute();

    }

    public override void Undo()
    {
        switch(pColor){
            case Piece.White:
                //if(!isWhiteCastlingAvail)
                  //  GameStateManager.Instance.isWhiteCastlingAvailable= true;
                break;
            case Piece.Black:
                // if(!isBlackCastlingAvail)
                //     GameStateManager.Instance.isBlackCastlingAvailable = true;
                    break;    
        }
        base.Undo();
        

    }

}
    
    public class EnPassantCommand : MoveCommand
    {   
        
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
            
            Console.WriteLine($"en Passant executed , caching the captured index -> {capturedPawnIndex}");
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
            Console.WriteLine($"Promoted to {promotedToPiece}" );
            
            currentBoard.chessBoard[targetCell] = promotedToPiece;
            currentBoard.chessBoard[currentCell] = Piece.Empty;
          //  engine.GetBoardClass.ShowBoard();
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
