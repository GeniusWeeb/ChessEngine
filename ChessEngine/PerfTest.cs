using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using Utility;

namespace ChessEngine;

public class PerfTest
{
    private Stopwatch watch = new Stopwatch();
    private bool firstScan = true;
    private readonly int customDepth = 2;
    private  int moveDelay = 300;
    private int finalpos = 0;
    int currentColor;
    int promCount = 0;
    private List<PieceThatCanMove> tempMoveList = new List<PieceThatCanMove>();
    private List<ShowMoveList> PerftList = new List<ShowMoveList>();
    
    public void PerFMoveFinal()
    {
        int nodes = 0;
        List<ChessPiece> startNodePieces = new List<ChessPiece>();
        Board board = ChessEngineSystem.Instance.GetBoardClass;
        //Console.WriteLine("First to move is " + GameStateManager.Instance.playerToMove);
        int playeMoveColor = GameStateManager.Instance.playerToMove;
        startNodePieces =board.GenerateMoves(board.chessBoard,playeMoveColor );
       
        
        foreach (var piece in startNodePieces) //Root node
        {
            foreach (var movesIndex in piece.allPossibleMovesIndex)
            {
                tempMoveList.Add(new PieceThatCanMove(piece , piece.GetCurrentIndex, movesIndex));
            }

        }
        
        watch.Start();
        foreach (PieceThatCanMove move in tempMoveList)
        {   
            Console.WriteLine($"starting new node and piece is  ->  {move.piece.GetPieceCode}");
            int[] b = (int[])ChessEngineSystem.Instance.GetBoardClass.chessBoard.Clone();
            ChessEngineSystem.Instance.GetBoardClass.MakeMoveClone( b ,move.oldIndex, move.newIndex,move.piece);
            b = (int[])ChessEngineSystem.Instance.GetBoardClass.chessBoard.Clone();
            ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor(" ---------------------------Main board is  ---------------------------" );
            ChessEngineSystem.Instance.GetBoardClass.ShowBoard(b);
            
            ChessEngineSystem.Instance.UpdateUIWithNewIndex(move.oldIndex, move.newIndex);
            
            nodes += RunPerft(customDepth - 1 ,  b);
            Console.WriteLine($"Total nodes here  {FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex)}-> {nodes}");
            PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex), nodes));
            
        }
        
        watch.Stop();
        Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Moves generated in  {(watch.ElapsedMilliseconds)}");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
      
        foreach (var move in PerftList)
        {
            finalpos += move.count;
            Console.WriteLine($"{move.moveName}- {move.count}");
        }
        
        #region Logs
            Console.WriteLine($"Total positions are {finalpos}");
            Console.WriteLine($"Captured pieces are {GameStateManager.Instance.captureCount}");
            Console.WriteLine($"Check count is {GameStateManager.Instance.checkCount}");
            Console.WriteLine($"enPassant count is {GameStateManager.Instance.enPassantMoves}"); 
            Console.WriteLine($"Promotion count is {GameStateManager.Instance.promotionCount}");
            Console.WriteLine($"Castling count is {GameStateManager.Instance.castlingCount}");
        Console.ResetColor();
        #endregion
        
    }

    bool IsPawnPromotion(int newIndex , int pieceCode)
    {
        Console.WriteLine("Code is "+  pieceCode + "for new index " + newIndex);
        int pCode = pieceCode & Piece.CPiece;
        if ((newIndex / 8 == 7 || newIndex / 8 == 0) && pCode == Piece.Pawn)
        {
            Console.WriteLine("returning true");
            return true;
        }
        
        return false;
    }
    
    public char? GetPromotedPieceCode(int code)
    {   
        
        int pCode = code & Piece.CPiece;
        switch (pCode)
        {
            case Piece.Bishop:
                return 'b';
            case Piece.Queen:
                return 'q';
            case Piece.Rook:
                return 'r';
            case Piece.Knight:
                return 'n';
        }

        return null;
    }


    private int RunPerft(int currentDepth , Board board)
    {  
        int nodeCount = 0;
        if (currentDepth == 0)
        {   
            return 1;
        }                      
        
        List<PieceThatCanMove> currentList = new List<PieceThatCanMove>();
        List<ChessPiece> possibleMoveList = new List<ChessPiece>();
        possibleMoveList =  board.GenerateMoves( (int)board.GetCurrentTurn,);
        foreach (var piece in possibleMoveList ) //Root node2
        {
            foreach (var movesIndex in piece.allPossibleMovesIndex)
            {
                currentList.Add(new PieceThatCanMove(piece , piece.GetCurrentIndex, movesIndex));
            }
        }
        foreach (var move in currentList)
        {     
            
            int[] b = (int[])board.Clone();
            ChessEngineSystem.Instance.GetBoardClass.MakeMoveClone( b,move.oldIndex, move.newIndex,move.piece);
            b=  (int[])board.Clone();
            ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor(" ---------------------------child leaves  ---------------------------" );
            ChessEngineSystem.Instance.GetBoardClass.ShowBoard(b);
            nodeCount += RunPerft(currentDepth-1,  b);
         
            Thread.Sleep((int)(moveDelay));
            //UnMakeMove();
            
        }
        return nodeCount;

    }


    private void UnMakeMove()
    {
        Stack<ICommand> moveHistory = ChessEngineSystem.Instance.moveHistory;
        if ( moveHistory.Count > 0)
        {
            ICommand lastMove = moveHistory.Pop();
            lastMove.Undo();
            GameStateManager.Instance.UpdateTurns();
            
        }
    }

}







public struct PieceThatCanMove
{
    public ChessPiece piece;
    public int oldIndex;
    public int newIndex;


    public PieceThatCanMove(ChessPiece thisPiece , int old, int newInd)
    {
        this.piece = thisPiece;
        this.oldIndex = old;
        this.newIndex = newInd;
    }
}

public struct ShowMoveList
{
    public string moveName;
    public int count;

    public ShowMoveList(string n, int co)
    {
        moveName = n;
        count = co;
    }
}