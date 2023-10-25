using ChessEngine;

namespace Utility;

/// <summary>
/// This will handle mapping from the Fen format to the array format
/// </summary>

public static  class FenMapper
{
    
    //Fen start from top left to right
    //We mostly only need the TILL THE INITIAL SPACE for BOARD REPRESENTATION
    public static int[] MapFen(string notation =  "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        int[] board = new int[64]; 
        string fen = notation.Split(" ")[0];
        Console.WriteLine(fen);
        int rank = 7, file = 0;
        
        //static define dictionary

        Dictionary<char, int> GetPeiceName = new Dictionary<char, int>()
        {
            ['k'] = Piece.King , 
            ['q'] = Piece.Queen,
            ['r'] = Piece.Rook,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['p'] = Piece.Pawn,
            
        };
        
        foreach (var c in fen )
        {
            if (c == '/')
            {
                file = 0;
                rank--;
            }
            else
            {
                if (char.IsDigit(c))
                { 
                    file += (int)char.GetNumericValue(c);
                }
                else
                {
                   int pieceType =  GetPeiceName[char.ToLower(c)];
                   int pieceColor = char.IsUpper(c) ? Piece.White : Piece.Black;

                   board[(8 * rank) + file] = pieceType | pieceColor;
                   file++;

                }
            }

        }
        
        return board;
    }




}