using ChessEngine;

namespace Utility;


public class GameStateManager
{
    public List<ChessPiece> pieces = new List<ChessPiece>();
    private TurnToMove toMove;
    private bool whiteToMove = true;
    
    public static GameStateManager Instance { get; private set; }
    public TurnToMove GetTurnToMove => toMove;
    public void SetTurnToMove(TurnToMove move)
    { 
        toMove = move;
    }
    public GameStateManager()
    {
        if(Instance == null) Instance = new GameStateManager();
    }
    
    //Entry point
    public void UpdateTurns()
    {
        ChangeTurns();
        //Update for later and send validation string for turn update alongside move validation
    }

    private bool ChangeTurns()
    { 
        whiteToMove = !whiteToMove;
        return whiteToMove;
    }







}



public enum TurnToMove
{
    black ,
    white
    
}