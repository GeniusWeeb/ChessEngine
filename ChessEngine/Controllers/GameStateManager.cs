using ChessEngine;

namespace Utility;


public class GameStateManager
{

    public List<ChessPiece> allPiecesThatCanMove = new List<ChessPiece>();
   
    private bool whiteToMove = true;
    private LegalMoves moves = new LegalMoves();

    public List<ChessPiece> pieces = new List<ChessPiece>();
    public int toMove = Piece.White;

    
    public static GameStateManager Instance { get; private set; }
    public int GetTurnToMove => toMove;
    public void SetTurnToMove(int move)
    { 
        toMove = move;
    }
    static  GameStateManager()
    {
        Instance = new GameStateManager();
    }
    

    
    //should be added in after turns are updated
    public void ProcessMoves(int[] board)
    {
        //Test colour to move 
        moves.CheckForMoves(board ,toMove);
        foreach (var piece in allPiecesThatCanMove)
        {
            Console.WriteLine($"piece code is {piece.GetPieceCode}- piece index is {piece.GetCurrentIndex}");
            foreach (var pieceIndex in piece.GetAllMovesForThisPiece )
            { 
                   Console.WriteLine("available move indexes are" + pieceIndex);
            } Console.WriteLine("-------------------------------------------------------------");
            
        }
    }

    //Entry point // when this updates -> Process moves here
  

    //Entry point
    public void UpdateTurns(int move )
    {   
        ChangeTurns(move);

        //Update for later and send validation string for turn update alongside move validation
    }

    private void  ChangeTurns(int move)
    {   
        
        Console.WriteLine("Changing turns");
        toMove = move == (int)Piece.White ?(int) Piece.Black :(int) Piece.White;
        Console.WriteLine("New move colour is" + toMove);
    }

    
    public void ResetMoves()
    {
        allPiecesThatCanMove.Clear();    
    }





}



