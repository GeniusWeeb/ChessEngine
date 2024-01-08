
using System.Diagnostics;
using System.Text;
using Tensorflow;
namespace ChessEngine
{   
    //MiniMax search algorithm : 
    public class SearchMinMax
    {
        public Move? FindBestMove(Board board , bool supremeBot , int defaultDepth = 3)
        {
            string modelPath = supremeBot ? "dumb.keras" : "smart.keras";
            List<Move> moveList = new List<Move>();
            int colorToScanFor = Piece.White;
            List<ChessPiece> startNodePieces = board.GenerateMoves(colorToScanFor,board,false );
            int bestEvalScore = int.MinValue;
            Move bestMove = null;
            
            foreach (var piece in startNodePieces) //Root node
            {
                foreach (var movesIndex in piece.allPossibleMovesIndex)
                {
                    moveList.Add(new Move( piece.GetCurrentIndex,  movesIndex,  piece));
                }
            }
            foreach (Move move in moveList)
            {
                Board board_cpy = new Board(board,BoardCloneTypes.MAIN);
                board_cpy.MakeMoveClone(move);
                //different for black and white
                int score =PerformMiniMax(board_cpy , defaultDepth, false, modelPath);
                if (score > bestEvalScore)
                {
                    bestEvalScore = score;
                    bestMove = move;
                }
            }
            
            return bestMove;
        }
        //return the Score
        private int PerformMiniMax(Board board , int depth , bool isMaximizingPlayer, string path)
        {
            string modelPath = path;
            if (depth == 0) return EvaluateBoard(board, modelPath); // or the GAME OVer state
            List<Move> tempLegalMoves = new List<Move>();
            List<ChessPiece> piecesThatCanMove = board.GenerateMoves(board.GetCurrentTurn, board, false);
            foreach (var piece in piecesThatCanMove) //Root node
            {
                foreach (var movesIndex in piece.allPossibleMovesIndex)
                {
                    tempLegalMoves.Add(new Move( piece.GetCurrentIndex,  movesIndex,  piece));
                }
            }

            if (isMaximizingPlayer)
            {
                int maxEval = int.MinValue;

                foreach (var move in tempLegalMoves)
                {
                    Board board_cpy = new Board(board,BoardCloneTypes.DepthCloning);
                    board_cpy.MakeMoveClone(move);
                    int eval = PerformMiniMax(board_cpy, depth - 1, false, modelPath);
                    maxEval = Math.Max(maxEval, eval);
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                
                foreach (var move in tempLegalMoves)
                {
                    Board board_cpy = new Board(board,BoardCloneTypes.DepthCloning);
                    board_cpy.MakeMoveClone(move);
                    //Since this is the black side or so, this will then update to white side which is 
                    // trying to maximize the value
                    int eval = PerformMiniMax(board_cpy, depth - 1, true, modelPath);
                    minEval = Math.Min(minEval, eval);
                }
                return minEval;
            }
        }
        private int EvaluateBoard(Board board , string modelPath)
        { 
            var model = Keras.Models.Model.LoadModel(modelPath);
            string fenIs = GenerateFenDataFromBoard(board);
            var input = FenToCombinedInput(fenIs);
            var takenInput = input.Take(768).ToArray();
            // Reshape the array
            var reshapedInput = ReshapeInputArray(takenInput, 1, 8, 8, 12);
            var eval =  model.Predict(reshapedInput);
            int evalFinal = (int)eval[0][0];
            Console.WriteLine($"Eval score is {evalFinal}");
            return evalFinal;
        }
        static sbyte[,,,] ReshapeInputArray(sbyte[] inputArray, int dim1, int dim2, int dim3, int dim4)
        {
            sbyte[,,,] reshapedArray = new sbyte[dim1, dim2, dim3, dim4];
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    for (int k = 0; k < dim3; k++)
                    {
                        for (int l = 0; l < dim4; l++)
                        {
                            reshapedArray[i, j, k, l] = inputArray[i * dim2 * dim3 * dim4 + j * dim3 * dim4 + k * dim4 + l];
                        }
                    }
                }
            }
            return reshapedArray;
        }
        
        private string GenerateFenDataFromBoard(Board board)
        {
            StringBuilder fenBuilder = new StringBuilder();
            int turnToMove = board.GetCurrentTurn;
            
            for (int rank = 7; rank >= 0; rank--)
            {
                int emptySquares = 0;
                for (int file = 0; file < 8; file++)
                {
                    int index = rank * 8 + file;
                    if (board.chessBoard[index] == 0) // Empty square
                    {
                        emptySquares++;
                    }
                    else
                    {
                        if (emptySquares > 0)
                        {
                            fenBuilder.Append(emptySquares);
                            emptySquares = 0;
                        }

                        char pieceChar = GetPieceChar(board.chessBoard[index]);
                        fenBuilder.Append(pieceChar);
                    }
                }

                if (emptySquares > 0)
                {
                    fenBuilder.Append(emptySquares);
                }

                if (rank > 0)
                {
                    fenBuilder.Append('/');
                }
            }


            string fen = fenBuilder + " " + board.GetCurrentTurn + " " + ConvertCastlingRightsToStings(board.castleRight);
            return fen ;
            //need turn to move  and castling rights

        }
        
        private char GetPieceChar(int piece)
        {
            // Implement your mapping of piece values to FEN characters
            // For example: 1 for white pawn, 2 for white knight, -1 for black pawn, etc.
            switch (piece)
            {
                case 17:
                    return 'p';
                case 18:
                    return 'r';
                case 19:
                    return 'n';
                case 20:
                    return 'b';
                case 22:
                    return 'q'; 
                case 24:
                    return 'k';
                //Black ->
                
                case 33:
                    return 'P';
                case 34:
                    return 'R';
                case 35:
                    return 'N';
                case 36:
                    return 'B';
                case 38:
                    return 'Q';
                case 40:
                    return 'K';
                // Add cases for other pieces as needed
                default:
                    return ' ';
            }
        }

        private string ConvertCastlingRightsToStings(CastlingRights rights)
        {

            StringBuilder castleRight = new StringBuilder();


            if (rights.whiteKingSideCastling)
                castleRight.Append('K');
            if (rights.whiteQueenSideCastling)
                castleRight.Append('Q');;
            if (rights.blackKingSideCastling)
                castleRight.Append('k');
            if (rights.blackQueenSideCastling)
                castleRight.Append('q');


            if (castleRight.Length == 0)
                castleRight.Append('-');

            return castleRight.ToString();
        }
        public static sbyte[] FenToCombinedInput(string fen)
        {
            string[] parts = fen.Split(' ');
            string board = parts[0];
            string turn = parts[1];
            string castling = parts[2];

            sbyte[,,] boardMatrix = new sbyte[8, 8, 12];
            Dictionary<char, int> pieceMap = new Dictionary<char, int>
            {
                { 'P', 0 }, { 'N', 1 }, { 'B', 2 }, { 'R', 3 }, { 'Q', 4 }, { 'K', 5 },
                { 'p', 6 }, { 'n', 7 }, { 'b', 8 }, { 'r', 9 }, { 'q', 10 }, { 'k', 11 }
            };

            for (int i = 0; i < 8; i++)
            {
                string row = board.Split('/')[i];
                for (int j = 0; j < 8; j++)
                {
                    char pieceChar = row[j];
                    if (pieceMap.ContainsKey(pieceChar))
                    {
                        boardMatrix[i, j, pieceMap[pieceChar]] = 1; // if a piece is present 
                        // we fill its position
                    }
                    else
                    {
                        for (int k = 0; k < 12; k++)
                        {
                            boardMatrix[i, j, k] = 12; // Empty spaces
                        }
                    }
                }
            }

            sbyte[] turnVector = new sbyte[2];
            turnVector[turn == "w" ? 0 : 1] = 1;

            // Castling options representation (4-bit binary)
            int castlingBits = 0;
            foreach (char c in castling)
            {
                if ("KQkq".Contains(c))
                {
                    castlingBits |= 1 << (c - 'K');
                }
            }

            // Combine representations
            sbyte[] combinedInput = boardMatrix.Cast<sbyte>().Concat(turnVector).Concat(new sbyte[] { (sbyte)castlingBits }).ToArray();

            return combinedInput;
        }
    


    }


    
    
    
   
}