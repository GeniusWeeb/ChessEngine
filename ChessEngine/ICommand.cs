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
        protected List<int> blackPieceOnBoard = new List<int>();
        protected List<int> whitePiecesOnBoard = new List<int>();
        public abstract void Execute();
        public abstract void Undo();

        public abstract (int, int)? GetInfo();


    }



public class MoveCommand : Command
{
   
   protected  int currentCell;
   protected int targetCell ;
   protected  int capturedPiece;
   protected ChessEngineSystem engine ;
  


    //promotion //en Passant


   public MoveCommand(int currnt ,int target , ChessEngineSystem eng )
    {
            this.currentCell = currnt ;
            this.targetCell = target ;
            this.engine = eng;
            blackPieceOnBoard = GameStateManager.Instance.blackPiecesIndexOnBoard.ToList();
            whitePiecesOnBoard = GameStateManager.Instance.whitePiecesIndexOnBoard.ToList();
            
            
        
    }


    //Processed in Engine from UI or the board itself
    public override void Execute()
    {   
        
        this.capturedPiece = engine.GetBoardClass.chessBoard[targetCell];
        engine.GetBoardClass.chessBoard[targetCell] = engine.GetBoardClass.chessBoard[currentCell];
        engine.GetBoardClass.chessBoard[currentCell] =  Piece.Empty ;
       

    }


    public override (int, int)? GetInfo()
    {
        return (currentCell, targetCell);

    }

    //Sent to board for UI .
    public override void Undo()
    {
        
         engine.GetBoardClass.chessBoard[currentCell] = engine.GetBoardClass.chessBoard[targetCell];
         engine.GetBoardClass.chessBoard[targetCell] =  capturedPiece;
         engine.UpdateUIWithNewIndex(targetCell , currentCell , capturedPiece);
         this.capturedPiece = Piece.Empty;
         
        
         GameStateManager.Instance.blackPiecesIndexOnBoard = blackPieceOnBoard.ToList();
         GameStateManager.Instance.whitePiecesIndexOnBoard = whitePiecesOnBoard.ToList();
         
       //  Console.WriteLine("Black pieces count cached is "+ blackPieceOnBoard.Count);


    }

}

    
public class CastlingCommand : Command
{
    
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

    public  CastlingCommand ( ChessEngineSystem eng, int KingD , int KingN , int pColor)
    {
        this.kingDefaultCell = KingD ;
        this.kingNewCell= KingN ;
        this.color =  pColor;
        this.engine = eng ;
        blackPieceOnBoard = GameStateManager.Instance.blackPiecesIndexOnBoard.ToList();
        whitePiecesOnBoard = GameStateManager.Instance.whitePiecesIndexOnBoard.ToList();
      
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
         GameStateManager.Instance.isBlackCastlingAvailable = false;
         engine.UpdateUIWithNewIndex(RookDefaultCell, RookNewCell);
         break;

            case Piece.White:   
                 if (kingNewCell > kingDefaultCell)  
                     PerformCastle(-1,7);
                 else 
                    PerformCastle(+1,0);      
                     
         Console.WriteLine("White Castling is confirmed");
                 GameStateManager.Instance.castlingCount += 1; 
         GameStateManager.Instance.isWhiteCastlingAvailable = false; 
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
        engine.GetBoardClass.chessBoard[kingDefaultCell]  = engine.GetBoardClass.chessBoard[kingNewCell];
        engine.GetBoardClass.chessBoard[RookDefaultCell] = engine.GetBoardClass.chessBoard[RookNewCell];
        engine.GetBoardClass.chessBoard[kingNewCell] = Piece.Empty;
        engine.GetBoardClass.chessBoard[RookNewCell] = Piece.Empty;
        GameStateManager.Instance.blackPiecesIndexOnBoard = blackPieceOnBoard.ToList();
        GameStateManager.Instance.whitePiecesIndexOnBoard = whitePiecesOnBoard.ToList();
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
            Console.WriteLine($"King new cell is  {kingNewCell}, Rook default cell is at 0 and  {engine.GetBoardClass.chessBoard[rookCell]}");
            engine.GetBoardClass.chessBoard[kingNewCell + step] = engine.GetBoardClass.chessBoard[rookCell];
            engine.GetBoardClass.chessBoard[kingNewCell] = engine.GetBoardClass.chessBoard[kingDefaultCell];
            engine.GetBoardClass.chessBoard[kingDefaultCell] = Piece.Empty;
            engine.GetBoardClass.chessBoard[rookCell] = Piece.Empty;
            RookNewCell = kingNewCell + step;
            this.RookDefaultCell = rookCell;
            var side = GameStateManager.Instance.playerToMove == Piece.White
                ? GameStateManager.Instance.whitePiecesIndexOnBoard
                : GameStateManager.Instance.blackPiecesIndexOnBoard;
            engine.GetBoardClass.UpdateSideIndex(RookDefaultCell, RookNewCell ,side  );
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
    
    


    
    private ChessEngineSystem engine;

    public RookMoveCommand (ChessEngineSystem engs,int old , int newCell ,int color)
    {
        this.engine = engs;
        this.currentCell = old ;
        this.targetCell =  newCell;
        this.pColor = color;
        this.targetCheck = old % 8 ; //file 0 or 7
        blackPieceOnBoard = GameStateManager.Instance.blackPiecesIndexOnBoard.ToList();
        whitePiecesOnBoard = GameStateManager.Instance.whitePiecesIndexOnBoard.ToList();
      
    }

    public override void Execute()
    {   

      capturedPiece = engine.GetBoardClass.chessBoard[targetCell];   
      engine.GetBoardClass.chessBoard[targetCell] = engine.GetBoardClass.chessBoard[currentCell];
      engine.GetBoardClass.chessBoard[currentCell] = Piece.Empty;
        
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

    engine.GetBoardClass.chessBoard[currentCell] = engine.GetBoardClass.chessBoard[targetCell];
    engine.GetBoardClass.chessBoard[targetCell] = capturedPiece;
   

    engine.UpdateUIWithNewIndex(targetCell, currentCell,capturedPiece );
    capturedPiece = Piece.Empty;


    GameStateManager.Instance.blackPiecesIndexOnBoard = blackPieceOnBoard.ToList();
    GameStateManager.Instance.whitePiecesIndexOnBoard = whitePiecesOnBoard.ToList();
    }

    public override (int, int)? GetInfo()
    {
        return null;
    }
}

public class kingMoveCommand : MoveCommand
{   
    int pColor;
    bool isWhiteCastling = true ;
    bool isBlackCastling = true;
    public kingMoveCommand(int currnt, int target, ChessEngineSystem eng , int color) : base(currnt, target, eng)
    {
        pColor = color;
        // = eng.Get//;
        blackPieceOnBoard = GameStateManager.Instance.blackPiecesIndexOnBoard.ToList();
        whitePiecesOnBoard = GameStateManager.Instance.whitePiecesIndexOnBoard.ToList();
    }

    public override void Execute()
    {
        switch(pColor)
        {
            case Piece.White:
                if(GameStateManager.Instance.isWhiteCastlingAvailable)
                    isWhiteCastling = GameStateManager.Instance.isWhiteCastlingAvailable = false;
                break;
            case Piece.Black:
                 if(GameStateManager.Instance.isBlackCastlingAvailable)    
                   isBlackCastling = GameStateManager.Instance.isBlackCastlingAvailable = false;
                break;    
        }

      base.Execute();

    }

    public override void Undo()
    {
        switch(pColor){
            case Piece.White:
                if(!isWhiteCastling)
                    GameStateManager.Instance.isWhiteCastlingAvailable= true;
                break;
            case Piece.Black:
                if(!isBlackCastling)
                    GameStateManager.Instance.isBlackCastlingAvailable = true;
                    break;    
        }
        base.Undo();

    }

}
    public class EnPassantCommand : MoveCommand
    {   
        
        private int capturedPawnIndex;
        
        public EnPassantCommand(int currnt, int target, ChessEngineSystem eng ,int capPawnIndex ) : base(currnt, target, eng)
        {
            capturedPawnIndex = capPawnIndex;
            // = eng.Get//;
            blackPieceOnBoard = GameStateManager.Instance.blackPiecesIndexOnBoard.ToList();
            whitePiecesOnBoard = GameStateManager.Instance.whitePiecesIndexOnBoard.ToList();
        }


        public override void Execute()
        {
            this.capturedPiece = engine.GetBoardClass.chessBoard[capturedPawnIndex];
            engine.GetBoardClass.chessBoard[targetCell] = engine.GetBoardClass.chessBoard[currentCell];
            engine.GetBoardClass.chessBoard[currentCell] =  Piece.Empty ;
            engine.GetBoardClass.chessBoard[capturedPawnIndex] =  Piece.Empty ; 
            Console.WriteLine($"en Passant executed , caching the captured index -> {capturedPawnIndex}");
            engine.UpdateUIWithNewIndex(capturedPawnIndex);
          
        }

        public override void Undo()
        {
            engine.GetBoardClass.chessBoard[currentCell] = engine.GetBoardClass.chessBoard[targetCell];
            engine.GetBoardClass.chessBoard[capturedPawnIndex] = capturedPiece;
            engine.GetBoardClass.chessBoard[targetCell] = Piece.Empty; //only because it is En Passant.
            GameStateManager.Instance.blackPiecesIndexOnBoard = blackPieceOnBoard.ToList();
            GameStateManager.Instance.whitePiecesIndexOnBoard = whitePiecesOnBoard.ToList();
            engine.UpdateUIWithNewIndex(targetCell , currentCell , capturedPiece, capturedPawnIndex);
            this.capturedPiece = Piece.Empty;
         

        }
    }    
