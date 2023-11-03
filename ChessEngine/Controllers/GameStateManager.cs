using System.Diagnostics;
using ChessEngine;
using ChessEngine.Bot;

namespace Utility;


public class GameStateManager
{
    //default setting
    private GameMode currentGameMode;


    public List<ChessPiece> allPiecesThatCanMove = new List<ChessPiece>();
    private LegalMoves moves = new LegalMoves();
    public List<ChessPiece> pieces = new List<ChessPiece>();
    public int player1Move = Piece.White;
    public int player2Col; // 

    public List<ChessPiece> OppAllPiecesThatCanMove = new List<ChessPiece>();
    
    public bool whiteKingInCheck = false;
    public bool blackKingInCheck = false;
    
    public bool isBlackCastlingAvailable = true;
    public bool isWhiteCastlingAvailable = true;
    
   
    public bool blackKingSideRookMoved = false;
    public bool blackQueenSideRookMoved = false;
    public bool whiteKingSideRookMoved = false;
    public bool whiteQueenSideRookMoved = false;

    
    
    
    public static GameStateManager Instance { get; private set; }
    public int GetTurnToMove =>player1Move;
   
    static  GameStateManager()
    {
        Instance = new GameStateManager();
    }
    
    public GameMode GetCurrentGameMode => currentGameMode;

    public void  SetCurrentGameModeAndTurn(string mode)
    {
       
        var gMode = mode.Split("-")[0];
        var pColChoice = mode.Split("-")[1];
        
        if (gMode.Equals(GameMode.PlayerVsPlayer.ToString()))
            this.currentGameMode = GameMode.PlayerVsPlayer;
        else if (gMode.Equals(GameMode.PlayerVsBot.ToString()))
            this.currentGameMode = GameMode.PlayerVsBot;
       
        
        player2Col = int.Parse(pColChoice);

    }
    
   // public  GameMode GetCurrentGameMode => currentGameMode;
    
    //should be added in after turns are updated
    public void ProcessMoves(int[] board)
    {
        // Checking  based on colour
        moves.CheckForMoves(board , player1Move , ref allPiecesThatCanMove);
 
    }
    
    
    //Overloading so that , even the Ai can scan the board at any time
    public void ProcessMoves(int[] board , int customToMove)
    {   
        //like passing a temporary  board state rather than  original and checking counter moves
        moves.CheckForMoves(board ,customToMove, ref  OppAllPiecesThatCanMove);
        
    }
    

    //Entry point // when this updates -> Process moves here
    public void UpdateTurns(int move )
    {   
        ChangeTurns(move);
        //Update for later and send validation string for turn update alongside move validation
    }

    private void  ChangeTurns(int move)
    {
       player1Move = move == (int)Piece.White ?(int) Piece.Black :(int) Piece.White;
    }

    
    public void ResetMoves()
    {
        allPiecesThatCanMove.Clear();   
        OppAllPiecesThatCanMove.Clear();
    }

    

}



public enum GameMode
{
    PlayerVsPlayer ,
    PlayerVsBot ,
    BotVsBot,
    None
}



