using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using Utility;

namespace ChessEngine;

public class PerfTest
{
    private Stopwatch watch = new Stopwatch();
    private bool firstScan = true;
    private readonly int customDepth = 5;
    private  int moveDelay = 0;
    private int finalpos = 0;
    int currentColor;
    int promCount = 0;
    private List<Move> moveList = new List<Move>();
    private List<ShowMoveList> PerftList = new List<ShowMoveList>();
    private int count;
    public void PerFMoveFinal()
    {
        int nodes = 0;
        List<ChessPiece> startNodePieces = new List<ChessPiece>();
        Board board = ChessEngineSystem.Instance.GetBoardClass;
        startNodePieces =board.GenerateMoves(board.GetCurrentTurn ,board , false );
        
        
        foreach (var piece in startNodePieces) //Root node
        {
            foreach (var movesIndex in piece.allPossibleMovesIndex)
            {
                moveList.Add(new Move( piece.GetCurrentIndex, movesIndex,  piece));
                //Console.WriteLine($"Move piece {piece.GetPieceCode} goes from {piece.GetCurrentIndex} to {movesIndex}");
            }

        }
        
     
        watch.Start();
        foreach (Move move in moveList)
        {
            count++;
                //    ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor(" ---------------------------Main board Move is  ---------------------------" );
            Board board_cpy = new Board(board,"clone");
            board_cpy.MakeMoveClone(move);
            ChessEngineSystem.Instance.UpdateUIWithNewIndex(move.from, move.to ,board_cpy.chessBoard[move.to] );
            
            nodes += RunPerft(customDepth - 1 , board_cpy);
            PerftList.Add(new ShowMoveList(FenMapper.IndexToAlgebric(move.from, move.to), nodes));
            nodes = 0;

        }
        
        watch.Stop();
        
        #region Logs
        
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"Moves generated in  {(watch.ElapsedMilliseconds/1000)} seconds");
                    Console.ResetColor();
                    
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


    private int RunPerft(int currentDepth ,  Board board)
    {  
        
        if (currentDepth == 0)
        {   
            return 1;
        }      
        
        int nodeCount = 0;
        
        List<Move> currentList = new List<Move>();
        List<ChessPiece> possibleMoveList = new List<ChessPiece>();
        possibleMoveList =  board.GenerateMoves( (int)board.GetCurrentTurn, board , false);
        foreach (var piece in possibleMoveList ) //Root node2
        {
            foreach (var movesIndex in piece.allPossibleMovesIndex)
            {
                currentList.Add(new Move(   piece.GetCurrentIndex, movesIndex, piece));
            }
        }
        foreach (var move in currentList)
        {
            

            Board board_cpy =  new Board(board , $"clone at  {currentDepth}");
            board_cpy.MakeMoveClone(move);
           // ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor(" ---------------------------child leaves  ---------------------------" );
            nodeCount += RunPerft(currentDepth-1,  board_cpy);
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