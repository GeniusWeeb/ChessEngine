
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
         CreateDefaultPawnIndex();
         GameStateManager.Instance.SetCurrentGameModeAndTurn(gameMode);
         var data = JsonConvert.SerializeObject(chessBoard);
         Protocols finalData = new Protocols(ProtocolTypes.GAMESTART.ToString(),data ,16.ToString());
         ChessEngineSystem.Instance.SendDataToUI(finalData);
         GameStateManager.Instance.ProcessMoves(ref chessBoard);
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
                PerformPostMoveCalculation( ChessEngineSystem.Instance, oldIndex , newIndex ,piece);
                ShowBoard();
                GameStateManager.Instance.ResetMoves();
                GameStateManager.Instance.UpdateTurns();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{p.GetPieceCode} moved from {oldIndex} to {newIndex}");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
                
            
                //Wrap the move in a command
                
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

    
    public  void ShowBoard()
     {
         Console.WriteLine(JsonConvert.SerializeObject(chessBoard));
     }


     public void Dispose()
     {
       

     }

     public void ProcessMovesUpdate()
     {  
         GameStateManager.Instance.ProcessMoves(ref chessBoard);
     }
     
     public void ProcessMovesUpdate(int[] customBoard , int turnToMove)
     {  
         GameStateManager.Instance.ProcessMoves(ref customBoard , turnToMove);
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
             if (pawnCode == Piece.Pawn)
             {
                 pawnDefaultIndex.Add(new PawnDefaultPos(ChessEngineSystem.Instance.GetColorCode(chessBoard[i]) ,i ));
                 Console.WriteLine($"${ChessEngineSystem.Instance.GetColorCode(chessBoard[i])} pawn saved at  {i}");
             }
             
         }
     }

     public ref int[] GetCurrentBoard()
     {
         return ref chessBoard;
     }

     public bool CheckIfPawnDefaultIndex(int pColor , int index)
     {

         foreach (var p in pawnDefaultIndex )
         {
             if (p.colorCode == pColor && p.indexCode == index)  // False if White piece at a Black piece default index
                 return true;
         }
         return false; 
     }


     public void PerformPostMoveCalculation ( ChessEngineSystem eng,int oldIndex,  int newIndex, int piece)
     { 
        
         
         int pCode = piece & Piece.CPiece;
         int pColor = ChessEngineSystem.Instance.IsBlack(piece) ? Piece.Black : Piece.White;
        

        switch(pCode)
        {

          case Piece.Queen:
          case Piece.Bishop:
          case Piece.Knight:
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
                    ICommand moveKing = new kingMoveCommand(oldIndex ,newIndex ,ChessEngineSystem.Instance , pColor);   
                    eng.ExecuteCommand(moveKing);  
                }
                    break;
          case Piece.Pawn:
                if(newIndex / 8 == 7 || newIndex / 8 == 0)
                    Console.WriteLine("Trying to promote");            
                else  {
                    ICommand movePawn = new MoveCommand(oldIndex,newIndex ,ChessEngineSystem.Instance);
                    eng.ExecuteCommand(movePawn);
                    }   
                    //What about en Passant??
                     break; 
          case Piece.Rook:
                    ICommand moveRook =  new RookMoveCommand(ChessEngineSystem.Instance,oldIndex , newIndex,pColor);
                    eng.ExecuteCommand(moveRook);
            break;                  
          
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
