
using System.Diagnostics;
using ChessEngine.Bot;
using Utility;
using Newtonsoft.Json;

namespace ChessEngine 
{
    // NOTE  :  This will  Handle BOARD CREATION 

 public class Board : IDisposable
 {
     private  int[] chessBoard;
     private List<int> pawnDefaultIndex = new List<int>();
     public BotBrain botBrain = new BotBrain();
   
     public Board()
     {
         chessBoard = new int[64];
         Console.WriteLine("Board is ready!");
         
         //Entry point , get the game mode and set it too .
         Event.ClientConncted += SetupDefaultBoard;
         
     }
        
    //HAPPENS AT THE TIME OF NEW BOARD -> COULD BE USED FOR A FORCE RESET 
    private void SetupDefaultBoard(string gameMode)
     {
         chessBoard = ChessEngineSystem.Instance.MapFen();//Board is ready at this point -> parsed from fen
         CreateDefaultPawnIndex();
         GameStateManager.Instance.SetCurrentGameModeAndTurn(gameMode);
         var data = JsonConvert.SerializeObject(chessBoard);
         Protocols finalData = new Protocols(ProtocolTypes.GAMESTART.ToString(),data ,16.ToString());
         ChessEngineSystem.Instance.SendDataToUI(finalData);
         GameStateManager.Instance.ProcessMoves(chessBoard);
         ChessEngineSystem.Instance.CheckForGameModeAndPerform();

     }
    
    public bool MakeMove( int oldIndex, int newIndex)
    {


        var piece = chessBoard[oldIndex];
        
        foreach (var p in GameStateManager.Instance.allPiecesThatCanMove)
        {
            
            if (p.GetPieceCode == piece && p.GetAllMovesForThisPiece.Contains(newIndex) &&
                oldIndex == p.GetCurrentIndex)
            {
               
                CheckForBonusBasedOnPieceCapture(piece,chessBoard[newIndex]);
                PerformPostMoveCalculation(oldIndex , newIndex ,piece);
                ShowBoard();
                GameStateManager.Instance.ResetMoves();
                GameStateManager.Instance.UpdateTurns(GameStateManager.Instance.player1Move);

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
    

    private void CheckForBonusBasedOnPieceCapture(int pieceThatMoved, int newIndex)
    
    {
        int pCode =newIndex & Piece.CPiece;
       // Console.WriteLine("PCode is that got captured" + pCode);
       // Console.WriteLine("Chessboard Index" + newIndex);
        
        switch (pCode)
        {
            case 0: 
               // Console.WriteLine("0 point for Empty");
                break;
            case 1: 
               // Console.WriteLine("1 point for Pawn");
                break;
            case 2:
               // Console.WriteLine("2 points for Rook");
                break;
            case 3: 
               // Console.WriteLine("3 points for Knight");
                break;
            case 4: 
               // Console.WriteLine("4 points for Bishop");
                break;
            case 6:// Console.WriteLine("6 points for Queen");
                break;
            
            
        }
    }

    
    private void ShowBoard()
     {
         Console.WriteLine(JsonConvert.SerializeObject(chessBoard));
     }


     public void Dispose()
     {
         Event.ClientConncted -= SetupDefaultBoard;

     }

     public void ProcessMovesUpdate()
     {  
         GameStateManager.Instance.ProcessMoves(chessBoard);
     }
     
     public void ProcessMovesUpdate(int[] customBoard , int turnToMove)
     {  
         GameStateManager.Instance.ProcessMoves(customBoard , turnToMove);
     }
     
    
     private void CreateDefaultPawnIndex()
     {
         int boardLength = chessBoard.Length;
         for (int i = 0; i < boardLength; i++)
         {  
             if(chessBoard[i] == Piece.Empty)
                 continue;
             //Storing the default pawn's index so we can perform the 2 square move
             int pawnCode = chessBoard[i] & Piece.CPiece;
             if(pawnCode == Piece.Pawn)
                 pawnDefaultIndex.Add(i);
         }
     }

     public int[] GetCurrentBoard => chessBoard;

     public bool CheckIfPawnDefaultIndex(int pawn)  => pawnDefaultIndex.Contains(pawn);


     public void PerformPostMoveCalculation(int oldIndex,  int newIndex, int piece)
     { 
        
         
         int pCode = piece & Piece.CPiece;
         int pColor = ChessEngineSystem.Instance.IsBlack(piece) ? Piece.Black : Piece.White;

         switch (pColor)
         {
             case 32: //BLACK
                 if (pCode == Piece.King)
                 {
                     if (MathF.Abs(newIndex - oldIndex) == 2)
                     {
                         int newRookIndex;
                         int oldRookIndex;
                         if (newIndex > oldIndex)
                         {
                             chessBoard[newIndex - 1] = chessBoard[63];
                             chessBoard[63] = Piece.Empty;
                             newRookIndex = newIndex-1;
                             oldRookIndex = 63;
                         }
                         else
                         {
                             chessBoard[newIndex + 1] = chessBoard[56];
                             chessBoard[56] = Piece.Empty;
                             newRookIndex = newIndex+1;
                             oldRookIndex = 56;
                         }
                         
                         Console.WriteLine("Black Castling is confirmed");
                         ChessEngineSystem.Instance.UpdateUIWithNewIndex(oldRookIndex, newRookIndex);
                     } // Confirm castling
                    
                     GameStateManager.Instance.isBlackCastlingAvailable = false;
                 }
                 else if (pCode == Piece.Rook)
                 {
                     Console.WriteLine("Rook moved , castling cancelled");
                     if (oldIndex % 8 == 7)
                         GameStateManager.Instance.blackKingSideRookMoved = true;
                     else
                         GameStateManager.Instance.blackQueenSideRookMoved = true;
                 }

                 break;
             case 16: //WHITE
                 if (pCode == Piece.King)
                 {  
                     if ( MathF.Abs( newIndex - oldIndex) == 2)
                     {
                         int newRookIndex;
                         int oldRookIndex;
                         if (newIndex > oldIndex)
                         {
                             chessBoard[newIndex - 1] = chessBoard[7];
                             chessBoard[7] = Piece.Empty;
                             newRookIndex = newIndex-1;
                             oldRookIndex = 7;
                         }
                         else
                         {
                             chessBoard[newIndex + 1] = chessBoard[0];
                             chessBoard[0] = Piece.Empty;
                             newRookIndex = newIndex + 1;
                             oldRookIndex = 0;
                         }
                         ChessEngineSystem.Instance.UpdateUIWithNewIndex( oldRookIndex, newRookIndex);
                         Console.WriteLine("White Castling is confirmed");
                     } // Confirm castling
                   
                     GameStateManager.Instance.isWhiteCastlingAvailable = false;
                 }

                 else if (pCode == Piece.Rook) {   
                     Console.WriteLine("Rook moved , castling cancelled");
                     if (oldIndex % 8 == 7)
                         GameStateManager.Instance.whiteKingSideRookMoved = true;
                     else
                         GameStateManager.Instance.whiteQueenSideRookMoved = true;
                 }
                 break;
         }
         
         
         chessBoard[oldIndex] = Piece.Empty;
         chessBoard[newIndex] = piece;
     }


   

 }   


}
