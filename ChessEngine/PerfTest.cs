using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using Utility;

namespace ChessEngine;

public class PerfTest
{
    private Stopwatch watch = new Stopwatch();
    private bool firstScan = true;
    private readonly int customDepth = 3;
    private  int moveDelay = 0;
    private int finalpos = 0;
    int currentColor;
    int promCount = 0;
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
        
        watch.Start();
        foreach (var move in tempList)
            
        {   
           
            ChessEngineSystem.Instance.GetBoardClass.MakeMoveTest(move.oldIndex, move.newIndex,move.piece);
            ChessEngineSystem.Instance.UpdateUIWithNewIndex(move.oldIndex, move.newIndex);
            
             if (IsPawnPromotion(move.newIndex , move.piece.GetPieceCode))
             {    
                 ChessEngineSystem.Instance.GetBoardClass.AddPromotionPieces();
                 Console.WriteLine("Found pawn for promotion");
                 int pColor = ChessEngineSystem.Instance.GetColorCode(move.piece.GetPieceCode);
                
               
                
                 int knight = ChessEngineSystem.Instance.GetBoardClass.promotionPieces.Pop() | pColor; 
                 ICommand promote1 = new PromotionCommand(move.oldIndex , move.newIndex, knight ,move.piece,ChessEngineSystem.Instance);
                 ChessEngineSystem.Instance.ExecuteCommand(promote1);
                 ChessEngineSystem.Instance.GetBoardClass.ShowBoard();
                 
                 promCount += RunPerft(customDepth - 1);
                 Thread.Sleep((int)(moveDelay));
                 Console.WriteLine(
                     $"{FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex) + GetPromotedPieceCode(knight)}" + promCount); 
                 PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex)+ GetPromotedPieceCode(knight),promCount));
                 UnMakeMove();
                
                 promCount = 0;
                 int rook = ChessEngineSystem.Instance.GetBoardClass.promotionPieces.Pop() | pColor;
                 Console.WriteLine($"Promoting rook {rook}");
                 ICommand promote2 = new PromotionCommand(move.oldIndex , move.newIndex, rook ,move.piece,ChessEngineSystem.Instance);
                 ChessEngineSystem.Instance.ExecuteCommand(promote2);
                 ChessEngineSystem.Instance.GetBoardClass.ShowBoard();
                 promCount += RunPerft(customDepth - 1);
                 Thread.Sleep((int)(moveDelay));
                 Console.WriteLine(
                     $"{FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex) + GetPromotedPieceCode(rook)}" + promCount);
                 PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex)+GetPromotedPieceCode(rook),promCount));
                 UnMakeMove();
            
                 promCount = 0;
                 int bishop = ChessEngineSystem.Instance.GetBoardClass.promotionPieces.Pop()| pColor;
                 ICommand promote3 = new PromotionCommand(move.oldIndex , move.newIndex, bishop ,move.piece,ChessEngineSystem.Instance);
                 ChessEngineSystem.Instance.ExecuteCommand(promote3);
                 ChessEngineSystem.Instance.GetBoardClass.ShowBoard();
                 promCount += RunPerft(customDepth - 1);
                 Thread.Sleep((int)(moveDelay));
                 Console.WriteLine(
                     $"{FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex) + GetPromotedPieceCode(bishop)}" + promCount);
                 PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex)+GetPromotedPieceCode(bishop),promCount));
                 UnMakeMove();

                 promCount = 0;
                 int queen = ChessEngineSystem.Instance.GetBoardClass.promotionPieces.Pop()| pColor;
                 ICommand promote4 = new PromotionCommand(move.oldIndex , move.newIndex, queen ,move.piece,ChessEngineSystem.Instance);
                 ChessEngineSystem.Instance.ExecuteCommand(promote4);
                 ChessEngineSystem.Instance.GetBoardClass.ShowBoard();
                 promCount += RunPerft(customDepth - 1);
                 Thread.Sleep((int)(moveDelay));
                 Console.WriteLine(
                     $"{FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex) + GetPromotedPieceCode(queen)}" + promCount);
                 PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex)+GetPromotedPieceCode(queen),promCount));
                 UnMakeMove();


                 foreach (var pr in ChessEngineSystem.Instance.GetBoardClass.promotionPieces)
                 {
                     Console.WriteLine("promotion piece is " + pr);
                 }
               

             }else {
                 nodes = RunPerft(customDepth - 1);
                 Thread.Sleep((int)(moveDelay));
                 PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.oldIndex, move.newIndex), nodes));
                 UnMakeMove();
             }  
             
             
           

        }
        
        watch.Stop();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Moves generated in  {(float)(watch.ElapsedMilliseconds)}");
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
        foreach (var piece in GameStateManager.Instance.allPiecesThatCanMove) //Root node2
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