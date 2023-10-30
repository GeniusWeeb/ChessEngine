using ChessEngine;

namespace Utility;


public class GameStateManager
{
    public List<ChessPiece> allPiecesThatCanMove = new List<ChessPiece>();
    private TurnToMove toMove;
    private bool whiteToMove = true;
    private LegalMoves moves = new LegalMoves();
    
    public static GameStateManager Instance { get; private set; }
    public TurnToMove GetTurnToMove => toMove;
    public void SetTurnToMove(TurnToMove move)
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
        int pToMove = Piece.White;
        
        //Test colour to move 
        
        moves.CheckForMoves(board ,pToMove);
        foreach (var piece in allPiecesThatCanMove)
        {
            Console.WriteLine($"piece code is {piece.GetPieceCode}- piece index is {piece.GetCurrentIndex}");
            foreach (var pieceIndex in piece.GetAllMovesForThisPiece )
            {   
                Console.WriteLine("available move indexes are" + pieceIndex);
            }
            Console.WriteLine("-------------------------------------------------------------");
            
        }
    }

    //Entry point // when this updates -> Process moves here
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

    
    public void ResetMoves()
    {
        allPiecesThatCanMove.Clear();    
    }





}



public enum TurnToMove
{
    black ,
    white
    
}