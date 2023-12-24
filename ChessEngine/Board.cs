
using System.Diagnostics;
using ChessEngine.Bot;
using Utility;
using Newtonsoft.Json;

namespace ChessEngine 
{
    // NOTE  :  This will  Handle BOARD CREATION 

 public class Board : IDisposable
 {
     public  int[] chessBoard;
     private List<PawnDefaultPos> pawnDefaultIndex = new List<PawnDefaultPos>();
  
   
     public Board()
     {
         chessBoard = new int[64];
         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine("Board is Initialised!");
         Console.ResetColor();
         //Entry point , get the game mode and set it too .
        
         
     }
        
    //HAPPENS AT THE TIME OF NEW BOARD -> COULD BE USED FOR A FORCE RESET 
    public  void SetupDefaultBoard(string gameMode)
     {
         
         Console.WriteLine("Trying to setup Default board");
         chessBoard = ChessEngineSystem.Instance.MapFen();//Board is ready at this point -> parsed from fen
         CreatePositionListForBothSides();
         GameStateManager.Instance.SetCurrentGameModeAndTurn(gameMode);
         var data = JsonConvert.SerializeObject(chessBoard);
         Protocols finalData = new Protocols(ProtocolTypes.GAMESTART.ToString(),data ,16.ToString());
         ChessEngineSystem.Instance.SendDataToUI(finalData);
         GameStateManager.Instance.ProcessMoves(ref chessBoard);
         ChessEngineSystem.Instance.CheckForGameModeAndPerform();
      

     }

    private void CreatePositionListForBothSides()
    {
        for (int i = 0; i < 64; i++)
        {
            var pColor = ChessEngineSystem.Instance.GetColorCode(chessBoard[i]);
            if(pColor == Piece.Empty ) continue;
            
            if(pColor == Piece.Black)
                GameStateManager.Instance.blackPiecesIndexOnBoard.Add(i);
            else 
                GameStateManager.Instance.whitePiecesIndexOnBoard.Add(i);
        }
    }
    
    

    public bool MakeMove( int oldIndex, int newIndex)
    {
        
        var piece = chessBoard[oldIndex];
        
        foreach (var p in GameStateManager.Instance.allPiecesThatCanMove)
        {   
            
            
            if (p.GetPieceCode == piece && p.GetAllMovesForThisPiece.Contains(newIndex) &&
                oldIndex == p.GetCurrentIndex)
            {
                int pColor = ChessEngineSystem.Instance.GetColorCode(p.GetPieceCode);

                var pCapCode = chessBoard[newIndex];
                PerformPostMoveCalculation( ChessEngineSystem.Instance, oldIndex , newIndex ,piece, p);
               
                UpdateSideIndex(oldIndex, newIndex,pColor == Piece.White ? 
                    GameStateManager.Instance.whitePiecesIndexOnBoard
                    : GameStateManager.Instance.blackPiecesIndexOnBoard );
                CheckForBonusBasedOnPieceCapture(piece, pCapCode,newIndex);
                ShowBoard();
                GameStateManager.Instance.ResetMoves();
                GameStateManager.Instance.UpdateTurns();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{p.GetPieceCode} moved from {oldIndex} to {newIndex}");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
               
                Console.ResetColor();
                
                return true;
                ; }
        }

        Console.WriteLine("Invalid Move");
            return false;
     }
    
    public void MakeMoveTest(int oldIndex, int newIndex, ChessPiece p  )
    {     
        var piece = chessBoard[oldIndex];
        int pColor = ChessEngineSystem.Instance.GetColorCode(p.GetPieceCode);

        var pCapCode = chessBoard[newIndex];
        PerformPostMoveCalculation( ChessEngineSystem.Instance, oldIndex , newIndex ,piece, p);
               
        UpdateSideIndex(oldIndex, newIndex,pColor == Piece.White ? 
            GameStateManager.Instance.whitePiecesIndexOnBoard
            : GameStateManager.Instance.blackPiecesIndexOnBoard );
        CheckForBonusBasedOnPieceCapture(piece, pCapCode,newIndex);
        GameStateManager.Instance.UpdateTurns();

        
    }


   public void UpdateSideIndex( int currentIndex,int newIndex , List<int> sideToUpdate)
    {   
        int updateIndex = -1;
        var tempList = sideToUpdate.ToList();

        for (int i = 0; i < tempList.Count; i++)
        {
            if (currentIndex != tempList[i]) continue;
            updateIndex = i;
            break;

        }

        Console.WriteLine($"Update index is  { updateIndex}");
        sideToUpdate[updateIndex] = newIndex;

    }

    void RemoveCapturedPieceFromSideIndex(int capIndex, List<int> sideToUpdate)
    {
        
        int updateIndex = 0;
        var tempList = sideToUpdate.ToList();

        for (int i = 0; i < tempList.Count; i++)
        {
            if (capIndex != tempList[i]) continue;
            updateIndex = i;
            break;

        }
        sideToUpdate.RemoveAt(updateIndex);
    }
    

    private void CheckForBonusBasedOnPieceCapture(int pieceThatMoved, int pieceCode , int capIndex )

    {


        int currentSide = GameStateManager.Instance.playerToMove;
        int pCode =pieceCode & Piece.CPiece;
     
      
       // Console.WriteLine("PCode is that got captured" + pCode);
       // Console.WriteLine("Chessboard Index" + newIndex);
        switch (pCode)
        {
            case 0:
               Console.WriteLine("0 point for Empty");
                break;
            case 1:
                GameStateManager.Instance.captureCount += 1;
                RemoveCapturedPieceFromSideIndex(capIndex, currentSide == Piece.White? GameStateManager.Instance.blackPiecesIndexOnBoard : GameStateManager.Instance.whitePiecesIndexOnBoard );
                
               // Console.WriteLine("1 point for Pawn");
                break;
            case 2:
                GameStateManager.Instance.captureCount += 1;
                RemoveCapturedPieceFromSideIndex(capIndex , currentSide == Piece.White? GameStateManager.Instance.blackPiecesIndexOnBoard : GameStateManager.Instance.whitePiecesIndexOnBoard);


               // Console.WriteLine("2 points for Rook");
                break;
            case 3: 
                GameStateManager.Instance.captureCount += 1;
                RemoveCapturedPieceFromSideIndex(capIndex , currentSide == Piece.White? GameStateManager.Instance.blackPiecesIndexOnBoard : GameStateManager.Instance.whitePiecesIndexOnBoard);


               // Console.WriteLine("3 points for Knight");
                break;
            case 4: 
                GameStateManager.Instance.captureCount += 1;
                RemoveCapturedPieceFromSideIndex(capIndex , currentSide == Piece.White? GameStateManager.Instance.blackPiecesIndexOnBoard : GameStateManager.Instance.whitePiecesIndexOnBoard);


               // Console.WriteLine("4 points for Bishop");
                break;
            
            case 6:// Console.WriteLine("6 points for Queen");
                GameStateManager.Instance.captureCount += 1;
                RemoveCapturedPieceFromSideIndex(capIndex , currentSide == Piece.White? GameStateManager.Instance.blackPiecesIndexOnBoard : GameStateManager.Instance.whitePiecesIndexOnBoard);
                
                break;
            
            
        }
    }

    
    public  void ShowBoard()
     {
         Console.ForegroundColor = ConsoleColor.Green;

         for (int i = 7; i >= 0; i--) // Start from the last row and go upwards
         {  
             
             for (int j = 0; j < 8; j++)
             {
             //    Console.Write($"{chessBoard[8 * i + j], -3} ");
                
             }
           //  Console.WriteLine();
         }

         Console.WriteLine($"white index is -> count is {GameStateManager.Instance.whitePiecesIndexOnBoard.Count } ");
         foreach (var index in GameStateManager.Instance.whitePiecesIndexOnBoard)
         {
             Console.Write($"{index , -3}");
         }
         Console.WriteLine("");
         Console.WriteLine($"black index is -> count is {GameStateManager.Instance.blackPiecesIndexOnBoard.Count}");
         foreach (var index in GameStateManager.Instance.blackPiecesIndexOnBoard)
         {
             Console.Write($"{index , -3}");
         }
         
         Console.ResetColor();
     }


     public void Dispose()
     {
       

     }

     public void ProcessMovesUpdate()
     {  
         GameStateManager.Instance.ProcessMoves(ref chessBoard);
     }
     
     public void ProcessMovesUpdate( int turnToMove)
     {  
         GameStateManager.Instance.ProcessMoves(ref chessBoard , turnToMove);
     }
     
     

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
     private void PerformPostMoveCalculation ( ChessEngineSystem eng,int oldIndex,  int newIndex, int piece,  ChessPiece p)
     {

         int pCode = piece & Piece.CPiece;
         int pColor = ChessEngineSystem.Instance.IsBlack(piece) ? Piece.Black : Piece.White;
         
        switch(pCode)
        {

          case Piece.Queen:
          case Piece.Bishop:
          case Piece.Knight:

              KingCheckCalculation(pColor,oldIndex,newIndex,p);
              ICommand justMove = new MoveCommand(oldIndex, newIndex, ChessEngineSystem.Instance);
              eng.ExecuteCommand(justMove);
              break;
          case Piece.King:

                if (MathF.Abs(newIndex - oldIndex) == 2)
                    {   //if castling
                        ICommand kingCastlingCommand = new CastlingCommand(ChessEngineSystem.Instance , oldIndex,newIndex  , pColor );
                        eng.ExecuteCommand(kingCastlingCommand); 
                    }
                else{ 
                    KingCheckCalculation(pColor,oldIndex,newIndex,p);
                    ICommand moveKing = new kingMoveCommand(oldIndex ,newIndex ,ChessEngineSystem.Instance , pColor);   
                    eng.ExecuteCommand(moveKing);  
                }
                    break;
          case Piece.Pawn:
              if (newIndex == p.specialIndex)
              {
                 
                  var capturedPawnIndex =  ChessEngineSystem.Instance.moveHistory.Peek().GetInfo();
                  if (capturedPawnIndex == null) return;
                  int  cellFinal = capturedPawnIndex.Value.Item2;
                  
                  KingCheckCalculation(pColor,oldIndex,newIndex,p);
                  ICommand enPassMoveCommand = new EnPassantCommand(oldIndex, newIndex, ChessEngineSystem.Instance, cellFinal);
                  eng.ExecuteCommand(enPassMoveCommand);
                  GameStateManager.Instance.enPassantMoves += 1;
                  Console.WriteLine($"Executed En Passant at {cellFinal}");
                  //Execute command and keep track
              }
              else if (newIndex / 8 == 7 || newIndex / 8 == 0)
              {
                  Console.WriteLine("Trying to promote , not right now");
                  KingCheckCalculation(pColor,oldIndex,newIndex,p);
                  ICommand movePawn = new MoveCommand(oldIndex,newIndex ,ChessEngineSystem.Instance);
                  eng.ExecuteCommand(movePawn);
                  
                  //What about Capture and promote?
              }            
                else  {
                  
                    KingCheckCalculation(pColor,oldIndex,newIndex,p);
                    ICommand movePawn = new MoveCommand(oldIndex,newIndex ,ChessEngineSystem.Instance);
                    eng.ExecuteCommand(movePawn);
                    }   
                  
                     break; 
          case Piece.Rook:
                    KingCheckCalculation(pColor,oldIndex,newIndex,p);
                    ICommand moveRook =  new RookMoveCommand(ChessEngineSystem.Instance,oldIndex , newIndex,pColor);
                    eng.ExecuteCommand(moveRook);
            break;
        }
        
        
     }

     //Opposite king
     private void KingCheckCalculation(int pColor, int oldIndex, int newIndex , ChessPiece p)
     {
         if (!IsOppKingInCheck(pColor, oldIndex, newIndex, p)) return; //Help us build on Undo
         if (GameStateManager.Instance.whiteKingInCheck == false && pColor == Piece.Black)
             GameStateManager.Instance.whiteKingInCheck = true;
         else if  (GameStateManager.Instance.blackKingInCheck == false && pColor == Piece.White)
             GameStateManager.Instance.blackKingInCheck = true;


         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine("White King" + GameStateManager.Instance.whiteKingInCheck);
         Console.WriteLine("Black King" + GameStateManager.Instance.blackKingInCheck);
         Console.ResetColor();
     }
    
     private  bool IsOppKingInCheck(int pColor , int oldIndex, int newIndex, ChessPiece p)
     {      
         int oppCol = GameStateManager.Instance.GetOpponent(pColor); // Opponent color
         int King = Piece.King | oppCol; //Opponent king

       
        // Console.WriteLine($"Checking if {oppCol} {King}  is  in check");
         int[] b =   (int[])chessBoard.Clone();
         b[oldIndex] = Piece.Empty;
         b[newIndex] = p.GetPieceCode;
        // Console.WriteLine("Oppoent col would be " + GameStateManager.Instance.GetOpponent(oppCol));
         GameStateManager.Instance.OppAllPiecesThatCanMove.Clear();
         ChessEngineSystem.Instance.CustomScanBoardForMoves(b , GameStateManager.Instance.GetOpponent(oppCol),"CUSTOM SCANNING IF OPP KING IN CHECK");
         foreach (var Pieces in GameStateManager.Instance.OppAllPiecesThatCanMove)
         {
             foreach (var movesIndex in Pieces.allPossibleMovesIndex)
             {
                 
               //  Console.WriteLine($"MOVES ARE {Pieces.GetPieceCode} To {movesIndex}");
                 if (chessBoard[movesIndex] == King)
                 {
                     Console.WriteLine($"{King}King is in check");
                     GameStateManager.Instance.checkCount += 1;
                     return true;
                 }
             }
         }

         return false;
     }
     
     //Check if my own king has been checked?
     public void KingBePreCheckTest(int[] board, int colCode)
     {
         int count = 0;
         bool hasLegalMoves = false;
         int checkOpponentMoves = GameStateManager.Instance.GetOpponent(colCode);

         foreach (var piece in GameStateManager.Instance.allPiecesThatCanMove.ToList()) // Create a copy to avoid modification during iteration
         {
             List<int> movesToRemove = new List<int>();

             foreach (int moveIndex in piece.allPossibleMovesIndex.ToList()) // Create a copy to avoid modification during iteration
             {
                 int[] b = (int[])board.Clone(); // Create a copy of the board

                 b[piece.GetCurrentIndex] = Piece.Empty;
                 b[moveIndex] = ChessEngineSystem.Instance.GetBoardClass.chessBoard[piece.GetCurrentIndex];

                 List<ChessPiece> tempPiecesThatCanMakeMove = new List<ChessPiece>();

                // Console.WriteLine($"Testing index {moveIndex}---------->");

                 // Temp test
                 GameStateManager.Instance.ProcessMoves(ref b, checkOpponentMoves, tempPiecesThatCanMakeMove);

                 foreach (var cp in tempPiecesThatCanMakeMove)
                 {
                     foreach (var OppmovIndex in cp.allPossibleMovesIndex)
                     {
                         if (b[OppmovIndex] == (Piece.King | GameStateManager.Instance.GetOpponent(checkOpponentMoves)))
                         {
                            
                           //  Console.WriteLine($" {colCode}King has been checked");
                             movesToRemove.Add(moveIndex);
                         //    Console.WriteLine($"Removed index {moveIndex} for check safety");
                             break; // Break the inner loop once a move is removed
                         }
                     }
                 }

                
             }

             //Added in main, performed functionlity on shallow and stored toRemove in a list
             //Removed from main list
             foreach (var moveToRemove in movesToRemove)
             {
                 piece.allPossibleMovesIndex.Remove(moveToRemove);
             }
             
             if (movesToRemove.Count < piece.getAllPossibleMovesCount)
                 hasLegalMoves = true;
             
             
         }
          
         
         foreach (var p in GameStateManager.Instance.allPiecesThatCanMove)
         {
             foreach (var mov in p.allPossibleMovesIndex)
             {
                 count += 1;
                // Console.WriteLine($"piece {p.GetPieceCode} can move to" + mov);
             }
         }

         Console.ForegroundColor = ConsoleColor.Cyan;
       //  Console.WriteLine("Final Filtered move list generated ");
       //  Console.WriteLine($" -< King PreCheck Filtering  -  {count}  ->");
         Console.ResetColor();
         
         if (!hasLegalMoves)
         {  
             
             ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor($"CHECKMATE {GameStateManager.Instance.GetOpponent(colCode)} wins");
             
             
            
         }
     }

   
   


 }




 public class PawnDefaultPos
 {
     //dont need pieceCode coz its either 17 or 33 so its constant//
     //This way we can map the white to the lower half that is 8-16 and black to the reverse order from  their default position.
     public  int colorCode;
     public  int indexCode;

     public PawnDefaultPos(int cCode , int index)
     {
         this.colorCode = cCode;
         this.indexCode = index;
     }
 }


}
