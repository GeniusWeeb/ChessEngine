
using ChessEngine;


namespace Utility;


public class GameStateManager
{
    //default setting
    private GameMode currentGameMode;

    public int promotionCount = 0;
    public int captureCount = 0;
    public int checkCount = 0;
    public int enPassantMoves = 0;
    public int castlingCount = 0;
    public int checkMateCount = 0;
    public HashSet<ChessPiece> allPiecesThatCanMove = new HashSet<ChessPiece>();
    public int? player1MoveCol;
    public int? player2MoveCol; // 
    
    
    //Default player to move -> Make black to make black move first
    public int playerToMove =  Piece.White;
    public bool whiteKingInCheck = false;
    public bool blackKingInCheck = false;
    public bool isBlackCastlingAvailable = true;
    public bool isWhiteCastlingAvailable = true;
    
   
    public bool blackKingSideRookMoved = false;
    public bool blackQueenSideRookMoved = false;
    public bool whiteKingSideRookMoved = false;
    public bool whiteQueenSideRookMoved = false;


    
    
    //k32 stands for KingSide castling for Black color , small k means black
    //Q32 stands for queenside castling for White color
    public void UpdateCastlingStateFromFen( bool K16 , bool Q16,bool k32 ,bool q32 )
    {
        whiteKingSideRookMoved = K16;
        whiteQueenSideRookMoved = Q16;

        blackKingSideRookMoved = k32;
        blackQueenSideRookMoved = q32;

    }
    //Both castling unavailable
    public void UpdateCastlingStateFromFen(bool WFalse , bool BFalse)
    {
        Console.WriteLine($"Castling allowed for white black? {WFalse}{BFalse}");
       
        isWhiteCastlingAvailable = WFalse;
        isBlackCastlingAvailable = BFalse;

    }



    public static GameStateManager Instance { get; private set; }
    public int GetTurnToMove =>playerToMove;


    public int GetOpponent(int currentTurn)

    {
       return currentTurn == Piece.White ? Piece.Black : Piece.White;
    }

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
        else if (gMode.Equals(GameMode.BotVsBot.ToString()))
            this.currentGameMode = GameMode.BotVsBot;
        else if (gMode.Equals(GameMode.PerfTest.ToString()))
            this.currentGameMode = GameMode.PerfTest;
       
        
        //ui side player
        player2MoveCol = int.Parse(pColChoice);

        player1MoveCol = player2MoveCol == playerToMove ? Piece.Black : Piece.White;



    }
    
    
    //Entry point // when this updates -> Process moves here
    public void UpdateTurns( )
    {   
        //Update for later and send validation string for turn update alongside move validation
        playerToMove = playerToMove == (int)Piece.White ?(int) Piece.Black :(int) Piece.White;
    }
    
    public void ResetMoves()
    {
        allPiecesThatCanMove.Clear();   
    }

    public void ResetGameState()
    {
     
     allPiecesThatCanMove.Clear();
     player1MoveCol = null;
     player2MoveCol = null;
     playerToMove = Piece.White;
     whiteKingInCheck = false;
     blackKingInCheck = false;
     isBlackCastlingAvailable = true;
     isWhiteCastlingAvailable = true;
     blackKingSideRookMoved = false;
     blackQueenSideRookMoved = false;
     whiteKingSideRookMoved = false;
     whiteQueenSideRookMoved = false;
     captureCount = 0;
     checkCount = 0;
     enPassantMoves = 0;
     castlingCount = 0;
    }


}




public enum GameMode
{
    PlayerVsPlayer ,
    PlayerVsBot ,
    BotVsBot,
    PerfTest,
    None
}



