
using System.Diagnostics;
using ChessEngine.Bot;
using Utility;
using Newtonsoft.Json;

namespace ChessEngine 
{
    // NOTE  :  This will  Handle BOARD CREATION 

 public class Board : IDisposable
 {

     public BoardCloneTypes name;
     public int[] chessBoard;
     private Turn currentTurn = Turn.White;
     public CastlingRights castleRight; //get from fen
     public readonly List<int> promoteToPieces = new List<int>()
     {
         
         Piece.Bishop,
         Piece.Knight,
         Piece.Rook,
         Piece.Queen,
     };
     
     public int blackKingCurrentIndex; 
     public int whiteKingKingCurrentIndex; 
     public Stack<ICommand> moveHistory;
     
     LegalMoves moves = new LegalMoves();
     Stopwatch watch = new Stopwatch();
    // public string enPassantSquare = "-"; --> en passant via fen not implemented -> Big pain

     public int GetCurrentTurn => (int)currentTurn;

     public void SetTurn(Turn turn) => currentTurn = turn;
     public Board(Board other, BoardCloneTypes cloneType = BoardCloneTypes.MAIN)
     {
         name = cloneType;
         chessBoard =  new int[64];
         for (int i = 0; i < 64; i++)
         {
             chessBoard[i] = other.chessBoard[i];
         }

         blackKingCurrentIndex = other.blackKingCurrentIndex;
         whiteKingKingCurrentIndex = other.whiteKingKingCurrentIndex;
         castleRight = new CastlingRights(other.castleRight); // copy constructor
         currentTurn = other.currentTurn;
         moveHistory = new Stack<ICommand>(other.moveHistory.Reverse());
         moves = new LegalMoves();
         watch = new Stopwatch();
       
         promoteToPieces = new List<int>()
         {
             Piece.Queen,
             Piece.Bishop,
             Piece.Knight,
             Piece.Rook
         };
     }
     
     public Board()
     {
         name = BoardCloneTypes.MAIN;
         castleRight = new CastlingRights(true, true, true, true);
         moveHistory = new Stack<ICommand>();
         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine($"  Board created ! ");
         Console.ResetColor();
         
     }
     
     public void ExecuteCommand(ICommand move)
     {      
         moveHistory.Push(move);
         move.Execute();
     }
     //needs to be debugged
     private void UndoCommand(string data)
     {
         if (moveHistory.Count == 0) return; // no  more moves to make
         GameStateManager.Instance.UpdateTurns();
         Console.ForegroundColor = ConsoleColor.Cyan;
         Console.ResetColor();
         ICommand lastMove = moveHistory.Pop();
         lastMove.Undo();
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine($"Received Undo Command showing board\n");
         Console.ResetColor();
         this.ShowBoard();
         GameStateManager.Instance.ResetMoves();
         this.GenerateMoves( (int)this.GetCurrentTurn, this, false);
         // ChessEngineSystem.Instance.CheckForGameModeAndPerform();

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
    
    public List<ChessPiece> GenerateMoves( int forThisColor , Board board ,bool justGen , bool isCustom = false)
    {
        if (justGen)
        {   List<ChessPiece> justAllMoves= new List<ChessPiece>();
            justAllMoves = moves.GenerateLegalMoves(board, forThisColor , isCustom );
            return justAllMoves;
        }
        else
        {   //else get legal moves as well
            List<ChessPiece> justAllMoves = new List<ChessPiece>();
            justAllMoves = moves.GenerateLegalMoves(board, forThisColor ,isCustom );
            return GetOnlyLegalMoves(justAllMoves, board, forThisColor);
        }
    } 
    
    //UIMakeMove -> Tailor it to look like the clone .
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
                UpdateTurns();
                PerformPostMoveCalculation( ChessEngineSystem.Instance, oldIndex , newIndex ,piece, p );
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{p.GetPieceCode} moved from {oldIndex} to {newIndex}");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
                Console.ResetColor();
                GameStateManager.Instance.allPiecesThatCanMove.Clear();
                return true;
                ; }
        }

        Console.WriteLine("Invalid Move");
            return false;
     }


    public bool BotMakeMove(Move move)
    {
        int piece =chessBoard[move.from];
        CheckForBonusBasedOnPieceCapture(piece,chessBoard[move.to]);
        UpdateTurns(); 
        PerformPostMoveCalculation( ChessEngineSystem.Instance, move.from, move.to ,piece, move.p);

        return true;
    }

    public void MakeMoveClone(Move move )
    {
        int piece =chessBoard[move.from];
        CheckForBonusBasedOnPieceCapture(piece,chessBoard[move.to]);
        UpdateTurns(); 
        PerformPostMoveCalculation( ChessEngineSystem.Instance, move.from, move.to ,piece, move.p);
    }


    public void UnMakeMove()
    {
        if (moveHistory.Count == 0)
            return;
        ICommand lastMove = moveHistory.Pop();
        lastMove.Undo();
        UpdateTurns();

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
 
    private void CheckForBonusBasedOnPieceCapture(int pieceThatMoved, int newIndex)
    {

        if (name == BoardCloneTypes.GetOnlyLegalMoves)
            return;
        
        int pCode =newIndex & Piece.CPiece;
       
        int myPieceCol = ChessEngineSystem.Instance.GetColorCode(pieceThatMoved);
      

        if (pCode == Piece.Empty)
            return;
        
       // Console.WriteLine("Chessboard Index" + newIndex);
        
        switch (pCode)
        {
            case 1:
                
                GameStateManager.Instance.UpdateCaptureCount();
                if (myPieceCol == Piece.Black)
                    GameStateManager.Instance.WhitePiecesCaptureCoint++;
                else
                    GameStateManager.Instance.BlackPiecesCapturedCount++;
               // Console.WriteLine("1 point for Pawn");
                break;
            case 2:
                if (myPieceCol == Piece.Black)
                    GameStateManager.Instance.WhitePiecesCaptureCoint++;
                else
                    GameStateManager.Instance.BlackPiecesCapturedCount++;
                GameStateManager.Instance.UpdateCaptureCount();
               // Console.WriteLine("2 points for Rook");
              
                break;
            case 3: 
                if (myPieceCol == Piece.Black)
                    GameStateManager.Instance.WhitePiecesCaptureCoint++;
                else
                    GameStateManager.Instance.BlackPiecesCapturedCount++;
                GameStateManager.Instance.UpdateCaptureCount();              

               // Console.WriteLine("3 points for Knight");
                break;
            case 4: 
                if (myPieceCol == Piece.Black)
                    GameStateManager.Instance.WhitePiecesCaptureCoint++;
                else
                    GameStateManager.Instance.BlackPiecesCapturedCount++;
                GameStateManager.Instance.UpdateCaptureCount();              

               // Console.WriteLine("4 points for Bishop");
                break;
            
            case 6:// Console.WriteLine("6 points for Queen");
                if (myPieceCol == Piece.Black)
                    GameStateManager.Instance.WhitePiecesCaptureCoint++;
                else
                    GameStateManager.Instance.BlackPiecesCapturedCount++;
                GameStateManager.Instance.UpdateCaptureCount();            

                break;
            
            
        }

        
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
     private void PerformPostMoveCalculation ( ChessEngineSystem eng,int oldIndex,  int newIndex, int piece,  ChessPiece p )
     {
            //Means this move has been confirmed
         int pCode = piece & Piece.CPiece;
         int pColor = ChessEngineSystem.Instance.IsBlack(piece) ? Piece.Black : Piece.White;
         
        switch(pCode)
        {

          case Piece.Queen:
          case Piece.Bishop:
          case Piece.Knight:

              ICommand justMove = new MoveCommand(oldIndex, newIndex, ChessEngineSystem.Instance , this, MoveType.Regular  );
              this.ExecuteCommand(justMove);
              KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
              break;
          case Piece.King:
                //Castling
                if (Math.Abs(newIndex - oldIndex) == 2)
                    {   //if castling
                        ICommand kingCastlingCommand = new CastlingCommand(ChessEngineSystem.Instance , oldIndex,newIndex  , pColor , this, MoveType.Castling);
                        this.ExecuteCommand( kingCastlingCommand); 
                        KingCheckCalculation(pColor,oldIndex,newIndex,pCode); }else
                { 
                    //Normal move
                    ICommand moveKing = new KingMoveCommand(oldIndex ,newIndex ,ChessEngineSystem.Instance , pColor ,  this , MoveType.Regular);   
                    this.ExecuteCommand(moveKing);  
                    KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                }
                    break;
          case Piece.Pawn:
              if (newIndex == p.specialIndex)
              {
                 //EnPassant
                  var capturedPawnIndex =  this. moveHistory.Peek().GetInfo();
                  if (capturedPawnIndex == null) return;
                  int  cellFinal = capturedPawnIndex.Value.Item2;
                  
                  ICommand enPassMoveCommand = new EnPassantCommand(oldIndex, newIndex, ChessEngineSystem.Instance, cellFinal,  this , MoveType.EnPassant);
                  this.ExecuteCommand(enPassMoveCommand);
                  
                  if(this.name != BoardCloneTypes.GetOnlyLegalMoves ) GameStateManager.Instance.enPassantMoves++;
                  
                
                  KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                //  Console.WriteLine($"Executed En Passant at {newIndex}");
                  //Execute command and keep track
              }
              else if (newIndex / 8 == 7 || newIndex / 8 == 0)
              {
                  Random ran = new Random();
                  int randIndecx = ran.Next(0, 4); //4 exclusive and 0 is inclusive
                  ICommand promPawnCommand = new PromotionCommand(oldIndex, newIndex,promoteToPieces[randIndecx], p, 
                      ChessEngineSystem.Instance, this, MoveType.Promotion);
                  this.ExecuteCommand(promPawnCommand);
                  KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                  //What piece to promote could use evaluation
              }            
                else  {
                  //normal pawn move
                    ICommand movePawn = new MoveCommand(oldIndex,newIndex ,ChessEngineSystem.Instance,  this , MoveType.Regular);
                    this.ExecuteCommand(movePawn);
                    KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                    } break; 
          case Piece.Rook:
                    ICommand moveRook =  new RookMoveCommand(ChessEngineSystem.Instance,oldIndex , newIndex,pColor,  this,MoveType.Regular );
                    this.ExecuteCommand(moveRook);
                    KingCheckCalculation(pColor,oldIndex,newIndex,pCode);
                    break;
        }
        
     }
     //Opposite king
     
     //Happens on main copy that is sent
     public void KingCheckCalculation(int pColor, int oldIndex, int newIndex ,int pCode)
     {  
         if (!IsOppKingInCheck(pColor, oldIndex, newIndex, pColor | pCode)) return; //Help us build on Undo


         if (name == BoardCloneTypes.GetOnlyLegalMoves)
             return;
         GameStateManager.Instance.checkCount++;
         //Console.WriteLine($"{pColor}King has been checked ");
     }

//after my move
     public int GetOpponentOfThis(int col)
     {
         return col == Piece.White ? Piece.Black : Piece.White;
     }

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
         {    //legal moves -> filter -> send a new hashset
             int myKing = Piece.King | forThisPlayer;
             foreach (ChessPiece piece in myLegalMoves.ToList())
             {  
                 List<int> movesToRemove = new List<int>();
                 foreach (int moveIndex in piece.allPossibleMovesIndex.ToList())
                 {   
                     Board boardCopy = new Board(board, BoardCloneTypes.GetOnlyLegalMoves);
                     // boardCopy.chessBoard[moveIndex] = piece.GetPieceCode;
                     // boardCopy.chessBoard[piece.GetCurrentIndex] = Piece.Empty;
                     boardCopy.MakeMoveClone(new Move(piece.GetCurrentIndex ,  moveIndex , piece));

                     //For opponent and get their ONLY LEGAL MOVES //1st case -> dep 1
                     List<ChessPiece>  oppsMovesList  = GenerateMoves( boardCopy.GetOpponentOfThis(forThisPlayer), boardCopy , true, true); //2 -> 

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

             int moveCount = 0;
             foreach (var move in myLegalMoves )
             {
                 moveCount += move.allPossibleMovesIndex.Count;
             }

             if (moveCount == 0)
                 GameStateManager.Instance.checkMateCount++;
             
          
             return myLegalMoves;


         }
         catch (Exception e)
         {
             Console.WriteLine(e);
             throw;
         }
     }

     public bool IsKingSafe(int forColor , int currentKingIndex)
     { 
         // color to check for and that ones index
         King king = new King(forColor , currentKingIndex);
         int OppCol = GetOpponentOfThis(forColor);   
        
         foreach (var index in  king.kingCanMoveTo)
         {
             int moveToIndex = king.GetCurrentIndex + index;

             while (moveToIndex >=0  && moveToIndex <64)
             {
                
              //   if(chessBoard[moveToIndex] == (Piece.Queen | OppCol ) ||  chessBoard[moveToIndex] == (Piece.Bishop))
              
              moveToIndex += index; 
             }
         }
         return true;
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


         public CastlingRights(CastlingRights other)
         {

             this.whiteKingSideCastling = other.whiteKingSideCastling;
             this.whiteQueenSideCastling = other.whiteQueenSideCastling;
             this.blackKingSideCastling = other.blackKingSideCastling;
             this.blackQueenSideCastling = other.blackQueenSideCastling;
         }

 }

 public enum MoveType
 {
     Regular , EnPassant, Promotion , Castling
 }

 public enum BoardCloneTypes
 {
  
     GetOnlyLegalMoves, 
     MAIN , 
     FILTERING,
     RookMoveClone,
     KingMoveClone,
     PromotionClone,
     DepthCloning
     
 }



}
