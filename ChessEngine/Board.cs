
using System.Diagnostics;
using ChessEngine.Bot;
using Utility;
using Newtonsoft.Json;

namespace ChessEngine 
{
    // NOTE  :  This will  Handle BOARD CREATION 

 public class Board : IDisposable
 {

     public int[] chessBoard;
     private Turn currentTurn = Turn.White;
     public CastlingRights castleRight = new CastlingRights(true, true, true, true); //get from fen
     
     LegalMoves moves = new LegalMoves();
     Stopwatch watch = new Stopwatch();
     
     

     public int GetCurrentTurn => (int)currentTurn;

     public void SetTurn(Turn turn) => currentTurn = turn;
     public Board(Board other, string type="main")
     {
         chessBoard =  new int[64];
         for (int i = 0; i < 64; i++)
         {
             chessBoard[i] = other.chessBoard[i];
         }

         currentTurn = other.currentTurn;
         moves = new LegalMoves();
         watch = new Stopwatch();

     }
     public Board()
     {
         
         
         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine($"  Board created ! ");
         Console.ResetColor();
         
     }
     
     //HAPPENS AT THE TIME OF NEW BOARD -> COULD BE USED FOR A FORCE RESET 
    public  void SetupDefaultBoard(string gameMode)
    {
       
         Console.WriteLine("Trying to setup Default board");
         var data = JsonConvert.SerializeObject(chessBoard);
         Protocols finalData = new Protocols(ProtocolTypes.GAMESTART.ToString(),data ,16.ToString());
         ChessEngineSystem.Instance.SendDataToUI(finalData);

         Console.ForegroundColor = ConsoleColor.Yellow;
         ShowBoard();
         Console.ResetColor();
         
    //     GenerateMoves( (int)currentTurn , this , false);
         

     }
    
    //If a method deosnt send depth , means its a firdst time search , 
    
    public List<ChessPiece> GenerateMoves( int forThisColor , Board board ,bool justGen )
    {
        if (justGen)
        {   List<ChessPiece> justAllMoves= new List<ChessPiece>();
            justAllMoves = moves.GenerateLegalMoves(board, forThisColor , false );
            return justAllMoves;

        }
        else
        {   //else get legal moves as well
            List<ChessPiece> justAllMoves = new List<ChessPiece>();
            justAllMoves = moves.GenerateLegalMoves(board, forThisColor , false );
            return GetOnlyLegalMoves(justAllMoves, board, forThisColor);
        }


        
        
    } 
    
    public bool MakeMove( int oldIndex, int newIndex)
    {
        Console.WriteLine("Checking UI ");
        var piece = chessBoard[oldIndex];
        
        foreach (var p in GameStateManager.Instance.allPiecesThatCanMove)
        {   
            
            
            if (p.GetPieceCode == piece && p.GetAllMovesForThisPiece.Contains(newIndex) &&
                oldIndex == p.GetCurrentIndex)
            {
                Console.WriteLine("Found move");
                CheckForBonusBasedOnPieceCapture(piece,chessBoard[newIndex]);
                PerformPostMoveCalculation( ChessEngineSystem.Instance, oldIndex , newIndex ,piece, p ,  this);
                //ShowBoard();
               // UpdateTurns();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{p.GetPieceCode} moved from {oldIndex} to {newIndex}");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
                Console.ResetColor();
                //Castling flag doesnt change yet
                // IsKingInCheck(chessBoard, 
                //     (GameStateManager.Instance.GetTurnToMove));
                //Check condition after move has been made
                return true;
                ; }
        }

        Console.WriteLine("Invalid Move");
            return false;
     }

    public void UnMakeMove(ICommand lastMove)
    {
        lastMove.Undo();
        
    }


    public void UpdateTurns()
    {
        currentTurn = currentTurn switch
        {
            Turn.White => Turn.Black,
            Turn.Black => Turn.White,
            _ => currentTurn
        };
    }

    public char? GetPromotedPieceCode(int code)
    {   
        
        int pCode = code & Piece.CPiece;
        switch (pCode)
        {
            case Piece.Bishop:
                return 'b';
            case Piece.Queen:
                return 'q';
            case Piece.Rook:
                return 'r';
            case Piece.Knight:
                return 'n';
        }

        return null;
    }

    public Stack<int> promotionPieces = new Stack<int>();

    public void AddPromotionPieces()
    {
        promotionPieces.Push(Piece.Queen);
        promotionPieces.Push(Piece.Bishop);
        promotionPieces.Push(Piece.Rook);
        promotionPieces.Push(Piece.Knight);
    }
    public void MakeMoveClone(Move move )
    {
        int piece =chessBoard[move.from];
        CheckForBonusBasedOnPieceCapture(piece,chessBoard[move.to]);
        UpdateTurns(); 
        PerformPostMoveCalculation( ChessEngineSystem.Instance, move.from, move.to ,piece, move.p, this);
    }

    private int CheckForBonusBasedOnPieceCapture(int pieceThatMoved, int newIndex)
    
    {
        int pCode =newIndex & Piece.CPiece;
    
       // Console.WriteLine("Chessboard Index" + newIndex);
        
        switch (pCode)
        {
            case 0:
               
               // Console.WriteLine("0 point for Empty");
                break;
            case 1:
                GameStateManager.Instance.captureCount += 1;
             
               // Console.WriteLine("1 point for Pawn");
                break;
            case 2:
                GameStateManager.Instance.captureCount += 1;

               // Console.WriteLine("2 points for Rook");
              
                break;
            case 3: 
                GameStateManager.Instance.captureCount += 1;
              

               // Console.WriteLine("3 points for Knight");
                break;
            case 4: 
                GameStateManager.Instance.captureCount += 1;
              

               // Console.WriteLine("4 points for Bishop");
                break;
            
            case 6:// Console.WriteLine("6 points for Queen");
                GameStateManager.Instance.captureCount += 1;
            

                break;
            
            
        }

        return 0;
    }

    
    public void ShowBoard()
     {
         Console.ForegroundColor = ConsoleColor.Green;

         for (int i = 7; i >= 0; i--) // Start from the last row and go upwards
         {  
             
             for (int j = 0; j < 8; j++)
             {
                 Console.Write($"{chessBoard[8 * i + j], -3} ");
                
             }
             Console.WriteLine();
         }
         Console.ResetColor();
     }
    
     public void Dispose()
     { }
     
     public ref int[] GetCurrentBoard()
     {
         return ref chessBoard;
     }

     public bool CheckIfPawnDefaultIndex(int pColor , int index)
     {
         switch (index / 8)
         {
             case 1 when pColor == Piece.White:
             case 6 when pColor == Piece.Black:
                 return true;
             
         }

         return false;
     }
     
     //Check for check on opponent king here
     private void PerformPostMoveCalculation ( ChessEngineSystem eng,int oldIndex,  int newIndex, int piece,  ChessPiece p , Board board)
     {
            //Means this move has been confirmed
         int pCode = piece & Piece.CPiece;
         int pColor = ChessEngineSystem.Instance.IsBlack(piece) ? Piece.Black : Piece.White;
         
        switch(pCode)
        {

          case Piece.Queen:
          case Piece.Bishop:
          case Piece.Knight:

              ICommand justMove = new MoveCommand(oldIndex, newIndex, ChessEngineSystem.Instance , board);
              eng.ExecuteCommand(justMove);
              KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
              break;
          case Piece.King:
                //Castling
                if (MathF.Abs(newIndex - oldIndex) == 2)
                    {   //if castling
                        ICommand kingCastlingCommand = new CastlingCommand(ChessEngineSystem.Instance , oldIndex,newIndex  , pColor , board);
                        eng.ExecuteCommand(kingCastlingCommand); 
                        KingCheckCalculation(pColor,oldIndex,newIndex,pCode); }
                else
                { 
                    //Normal move
                    ICommand moveKing = new kingMoveCommand(oldIndex ,newIndex ,ChessEngineSystem.Instance , pColor ,  board);   
                    eng.ExecuteCommand(moveKing);  
                    KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                }
                    break;
          case Piece.Pawn:
              if (newIndex == p.specialIndex)
              {
                 
                  var capturedPawnIndex =  ChessEngineSystem.Instance.moveHistory.Peek().GetInfo();
                  if (capturedPawnIndex == null) return;
                  int  cellFinal = capturedPawnIndex.Value.Item2;
                  
                  ICommand enPassMoveCommand = new EnPassantCommand(oldIndex, newIndex, ChessEngineSystem.Instance, cellFinal,  board);
                  eng.ExecuteCommand(enPassMoveCommand);
                  GameStateManager.Instance.enPassantMoves += 1;
                  KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                  Console.WriteLine($"Executed En Passant at {cellFinal}");
                  //Execute command and keep track
              }
              else if (newIndex / 8 == 7 || newIndex / 8 == 0)
              {
                   
                  Console.WriteLine($"Trying to promote , not right now -< {newIndex}");
                 
                  GameStateManager.Instance.promotionCount += 1;
               
              }            
                else  {
                  
                  
                    ICommand movePawn = new MoveCommand(oldIndex,newIndex ,ChessEngineSystem.Instance,  board);
                    eng.ExecuteCommand(movePawn);
                    KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                    } break; 
          case Piece.Rook:
                   
                    ICommand moveRook =  new RookMoveCommand(ChessEngineSystem.Instance,oldIndex , newIndex,pColor,  board);
                    eng.ExecuteCommand(moveRook);
                    KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
            break;
        }
        
        
     }
     //Opposite king
     public void KingCheckCalculation(int pColor, int oldIndex, int newIndex ,int pCode)
     {
         if (!IsOppKingInCheck(pColor, oldIndex, newIndex, pColor | pCode)) return; //Help us build on Undo

         GameStateManager.Instance.checkCount++;
         //Console.WriteLine($"{pColor}King has been checked ");
     }

//after my move

     public int getOpponent => (int)currentTurn == Piece.White ? Piece.Black : Piece.White;
     private  bool IsOppKingInCheck(int pColor , int oldIndex, int newIndex, int pCode)
     {      
       
         int oppCol = GameStateManager.Instance.GetOpponent(pColor); // Opponent color
         int King = Piece.King | oppCol; //Opponent king
         List<ChessPiece> oppPieceList = new List<ChessPiece>();

         Board board_cpy = this;
         board_cpy.chessBoard[oldIndex] = Piece.Empty;
         board_cpy.chessBoard[newIndex] = pCode;
         oppPieceList =  GenerateMoves(pColor, board_cpy , true );
         foreach (var piece in oppPieceList)
         {
             foreach (var movesIndex in piece.allPossibleMovesIndex)
             {

                 //  Console.WriteLine($"MOVES ARE {Pieces.GetPieceCode} To {movesIndex}");
                 if (board_cpy.chessBoard[movesIndex] != King) continue;
                 return true;
             }
         }

         return false;
     }

     public List<ChessPiece> GetOnlyLegalMoves(List<ChessPiece> myLegalMoves, Board board, int forThisPlayer) // sameSideColor
     {
         //2- 1 => depth
         try
         {
             //legal moves -> filter -> send a new hashset
             int myKing = Piece.King | forThisPlayer;
           
           
             foreach (ChessPiece piece in myLegalMoves.ToList())
             {  
                 List<int> movesToRemove = new List<int>();
                 foreach (int moveIndex in piece.allPossibleMovesIndex.ToList())
                 {   
                     Board boardCopy = new Board(board, "filterFinalMovesClone");
                     boardCopy.chessBoard[moveIndex] = piece.GetPieceCode;
                     boardCopy.chessBoard[piece.GetCurrentIndex] = Piece.Empty;

                     //For opponent and get their ONLY LEGAL MOVES //1st case -> dep 1
                     List<ChessPiece> oppsMovesList = new List<ChessPiece>();
                     oppsMovesList  = GenerateMoves( boardCopy.getOpponent , boardCopy , true); //2 -> 

                     foreach (ChessPiece oppPiece in oppsMovesList)
                     {
                         foreach (int canMoveToIndex in oppPiece.allPossibleMovesIndex)
                         {
                             if (boardCopy.chessBoard[canMoveToIndex] ==
                                 myKing) // Found my king on a deep check // Deep check here // Pinned in some way
                             {
                                 //Console.WriteLine($"King CHECKED REMOVING for {piece.GetPieceCode} at {moveIndex}");
                                 movesToRemove.Add(moveIndex);
                                 break;
                             }
                         }
                     }
                 }

                 foreach (var movesTo in movesToRemove)
                 {
                     piece.allPossibleMovesIndex.Remove(movesTo);
                 }
             }
             
             return myLegalMoves;


         }
         catch (Exception e)
         {
             Console.WriteLine(e);
             throw;
         }
     }

 }


 public class Move
 {
     public ChessPiece p; 
     public int from;
     public int to;

     public Move(int fromIndex, int toIndex, ChessPiece piece)
     {
         from = fromIndex;
         to = toIndex;
         p = piece;
     }
 }



 public enum Turn
 { 
     White =16 ,
     Black = 32 
    
 }


 public class CastlingRights
 {
     
         public bool whiteKingSideCastling;
         public bool whiteQueenSideCastling;
         
         
         public bool blackKingSideCastling;
         public bool blackQueenSideCastling;


         public CastlingRights(bool whiteKingSide , bool whiteQueenSide , bool blackKingSide ,bool blackQueenSide)
         {

             this.whiteKingSideCastling = whiteKingSide;
             this.whiteQueenSideCastling = whiteQueenSide;
             this.blackKingSideCastling = blackKingSide;
             this.blackQueenSideCastling = blackQueenSide;
             
         }

 }



}
