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
                    int pieceType = GetPieceCode(c);
                   int pieceColor = char.IsUpper(c) ? Piece.White : Piece.Black;

                   board[(8 * rank) + file] = pieceType | pieceColor;
                   file++;
                }
            }
        }
        
        return board;
    }

    public static int GetPieceCode(char c)
    {
        
        Dictionary<char, int> GetPeiceName = new Dictionary<char, int>()
        {
            ['k'] = Piece.King , 
            ['q'] = Piece.Queen,
            ['r'] = Piece.Rook,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['p'] = Piece.Pawn,
            
        };
        
         return GetPeiceName[char.ToLower(c)];

    }


    public static (int, int) AlgebricToBoard(string code)
    {   
          
        Dictionary<char, int> GetFileCode = new Dictionary<char, int>()
        {
            ['a'] = 0 , 
            ['b'] = 1,
            ['c'] = 2,
            ['d'] = 3,
            ['e'] = 4,
            ['f'] = 5,
            ['g'] = 6,
            ['h'] = 7,
            
        };
        
        #region oldPosition
            int file = GetFileCode[code[0]];
            int rank = (int)char.GetNumericValue(code[1]) -1;
            Console.WriteLine(rank);
            Console.WriteLine(file);
            int oldIndex = (8 * rank) + file; 
        #endregion
        
        #region newPosition
            int file1 = GetFileCode[code[2]];
            int rank1 = (int)char.GetNumericValue(code[3]) -1;
            int newIndex = (8 * rank1) + file1;
        #endregion
        Console.WriteLine("old Index is =>" + oldIndex +", new  Index is => " +newIndex);

        return (oldIndex,newIndex);
    }



}