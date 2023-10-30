using ChessEngine;

namespace Utility;


public class GameStateManager
{

    public List<ChessPiece> allPiecesThatCanMove = new List<ChessPiece>();
   
    private bool whiteToMove = true;
    private LegalMoves moves = new LegalMoves();

    public List<ChessPiece> pieces = new List<ChessPiece>();
    private int toMove;

    
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

    
    public void ResetMoves()
    {
        allPiecesThatCanMove.Clear();    
    }





}



