using ChessEngine;

namespace Utility;


public class GameStateManager
{
    public List<ChessPiece> pieces = new List<ChessPiece>();
    private int toMove;
    
    public static GameStateManager Instance { get; private set; }
    public int GetTurnToMove => toMove;
    public void SetTurnToMove(int move)
    { 
        toMove = move;
    }
    public GameStateManager()
    {
        if(Instance == null) Instance = new GameStateManager();
    }
    
    //Entry point
    public void UpdateTurns(int move )
    {   
        ChangeTurns(move);
        //Update for later and send validation string for turn update alongside move validation
    }

    private void  ChangeTurns(int move)
    {
        toMove = move == Piece.White ? Piece.Black : Piece.Black;
    }







}



