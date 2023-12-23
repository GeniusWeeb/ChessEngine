using System.Data;
using System.Security.Cryptography;
using Utility;

namespace ChessEngine;

public class PerfTest
{
   
    private bool firstScan = true;
    private readonly int customDepth = 1;
    private  int moveDelay = 500;
    private int finalpos = 0;
    int currentColor;
    private List<PieceThatCanMove> tempList = new List<PieceThatCanMove>();
    private List<ShowMoveList> PerftList = new List<ShowMoveList>();
    public void PerFMoveFinal()
    {
        int nodes = 0;
        GameStateManager.Instance.allPiecesThatCanMove.Clear();
        Console.WriteLine("First to move is " + GameStateManager.Instance.playerToMove);
        ChessEngineSystem.Instance.ScanBoardForMoves();
        
        foreach (var piece in GameStateManager.Instance.allPiecesThatCanMove) //Root node
        {
            foreach (var movesIndex in piece.allPossibleMovesIndex)
            {
                tempList.Add(new PieceThatCanMove(piece , piece.GetCurrentIndex, movesIndex));
            }

        }
        
        foreach (var move in tempList)
        {
            
            ChessEngineSystem.Instance.GetBoardClass.MakeMoveTest(move.oldIndex, move.newIndex,move.piece);
            ChessEngineSystem.Instance.UpdateUIWithNewIndex(move.oldIndex, move.newIndex);
            nodes =RunPerft(customDepth-1);
            Thread.Sleep((int)(moveDelay));
            PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex),nodes));
            UnMakeMove();
                
        }
        

        Console.ForegroundColor = ConsoleColor.Yellow;
      
        foreach (var move in PerftList)
        {
            finalpos += move.count;
            Console.WriteLine($"{move.moveName}- {move.count}");
        }
        Console.WriteLine($"Total positions are {finalpos}");
        Console.WriteLine($"Captured pieces are {GameStateManager.Instance.captureCount}");
        Console.WriteLine($"Check count is {GameStateManager.Instance.checkCount}");
        Console.WriteLine($"enPassant count is {GameStateManager.Instance.enPassantMoves}");
        Console.WriteLine($"Castling count is {GameStateManager.Instance.castlingCount}");
        Console.ResetColor();

    }
    

    private int RunPerft(int currentDepth )
    {   
       
        int nodeCount = 0;
        if (currentDepth == 0)
        {   
            return 1;
        }
     
      
        List<PieceThatCanMove> currentList = new List<PieceThatCanMove>();
        GameStateManager.Instance.allPiecesThatCanMove.Clear();
        ChessEngineSystem.Instance.ScanBoardForMoves();
        foreach (var piece in GameStateManager.Instance.allPiecesThatCanMove) //Root node
        {
            foreach (var movesIndex in piece.allPossibleMovesIndex)
            {
                currentList.Add(new PieceThatCanMove(piece , piece.GetCurrentIndex, movesIndex));
            }
        }
        foreach (var move in currentList)
        {
                
            ChessEngineSystem.Instance.GetBoardClass.MakeMoveTest(move.oldIndex, move.newIndex,move.piece);
            ChessEngineSystem.Instance.UpdateUIWithNewIndex(move.oldIndex, move.newIndex);
            nodeCount += RunPerft(currentDepth-1);
            Thread.Sleep((int)(moveDelay));
            UnMakeMove();
          
                
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