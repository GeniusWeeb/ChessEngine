using ChessEngine;

namespace Utility;


public class GameStateManager
{

    public List<ChessPiece> allPiecesThatCanMove = new List<ChessPiece>();
    private LegalMoves moves = new LegalMoves();
    public List<ChessPiece> pieces = new List<ChessPiece>();
    public int toMove = Piece.White;


    public bool WhiteKingHasBeenChecked = false;
    public bool BlackKingHasBeenChecked = false;
    public bool WhiteKingInCheck = false;
    public bool BlackKingInCheck = false;
    
    
    
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
        // Checking  based on colour
        moves.CheckForMoves(board ,toMove);
 
    }
    
    
    //Overloading so that , even the Ai can scan the board at any time
    public void ProcessMoves(int[] board , int customToMove)
    {   
        //like passing a temporary  board state rather than  original and checking counter moves
        moves.CheckForMoves(board ,customToMove);
    }
    

    //Entry point // when this updates -> Process moves here
    public void UpdateTurns(int move )
    {   
        ChangeTurns(move);
        //Update for later and send validation string for turn update alongside move validation
    }

    private void  ChangeTurns(int move)
    {
        toMove = move == (int)Piece.White ?(int) Piece.Black :(int) Piece.White;
    }

    
    public void ResetMoves()
    {
        allPiecesThatCanMove.Clear();    
    }





}



