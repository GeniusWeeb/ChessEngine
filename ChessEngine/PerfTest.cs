using System.Data;
using Utility;

namespace ChessEngine;

public class PerfTest
{
    private bool firstScan = true;
    private int customDepth = 2;
    private readonly int moveDelay = 0;
    private int finalpos = 0;
    int currentColor;
    private List<PieceThatCanMove> tempList = new List<PieceThatCanMove>();
    public void PerFMoveFinal()
    {
        
        StockFishAnalysisResults.moveCellsList.Clear();

        finalpos = (int)MoveGen(customDepth);
        Console.WriteLine($"Amount of positions generated {finalpos}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"Captured Pieces are  {StockFishAnalysisResults.capturedPieceCount}");
     
        
        
        Console.ResetColor();
        Console.WriteLine("Showing all moves");
        foreach (var moveData in  StockFishAnalysisResults.moveCellsList)
        {
            Console.WriteLine($"{moveData.name} : {moveData.count}");
        }
        
    }


    private int MoveGen(int depth)
    {
        
        if (depth == 0)
            return 1;
        int numOfPositions = 0;

        if (!firstScan  )
        {
            if (GameStateManager.Instance.playerToMove == currentColor)
                    
            { GameStateManager.Instance.UpdateTurns();
                GameStateManager.Instance.allPiecesThatCanMove.Clear();
                ChessEngineSystem.Instance.ScanBoardForMoves(); }

        }

        List<PieceThatCanMove> moveList = new List<PieceThatCanMove>();

        foreach (var pieces in GameStateManager.Instance.allPiecesThatCanMove)
        {
                foreach (var moves in pieces.allPossibleMovesIndex)
                {

                    moveList.Add(new PieceThatCanMove(pieces, pieces.GetCurrentIndex, moves));
                }
            }

        if (firstScan)
        {
            tempList = moveList;
            firstScan = false;
        }

        foreach (var p in moveList)
        {
            try
            {
               this. currentColor = ChessEngineSystem.Instance.GetColorCode(p.piece.GetPieceCode);
                // Make the move
                
                
                ChessEngineSystem.Instance.GetBoardClass.MakeMoveTest(p.oldIndex, p.newIndex, p.piece);
                StockFishAnalysisResults.AddToResults( FenMapper.IndexToAlgebric(p.oldIndex, p.newIndex));

                ChessEngineSystem.Instance.UpdateUIWithNewIndex(p.oldIndex, p.newIndex);

                // Recursively explore moves at the next depth
                numOfPositions += MoveGen(depth - 1);


                // Undo the move (backtrack)
                Thread.Sleep(moveDelay); // Optional delay for visualization purposes

                if (ChessEngineSystem.Instance.moveHistory.Count > 0)
                {
                    ICommand command = ChessEngineSystem.Instance.moveHistory.Pop();
                    command.Undo();
                }

                Console.WriteLine("----------------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Showing board after undo");
                ChessEngineSystem.Instance.GetBoardClass.ShowBoard();
                Console.ResetColor();
                Console.WriteLine("----------------------------------------------------------------");
                
                Console.WriteLine($"Move count left  -> {ChessEngineSystem.Instance.moveHistory.Count}");
                
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            

        }

            
            
        
        return numOfPositions;
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

