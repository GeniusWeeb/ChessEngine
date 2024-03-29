using ChessEngine;


namespace Utility;

/// <summary>
/// This will handle mapping from the Fen format to the array format
/// </summary>

public static  class FenMapper
{
  //  private static string defaultFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    
    
    //Fen start from top left to right
    //We mostly only need the TILL THE INITIAL SPACE for BOARD REPRESENTATION
    public static int[] MapFen(string notation = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    {
        int[] board = new int[64]; 
        string fen = notation.Split(" ")[0];
        string turn = notation.Split(" ")[1];
        string castlingData = notation.Split(" ")[2];
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
        
        bool K16 = false, Q16 = false, k32 = false, q32 = false;


        for (int i = 0; i < castlingData.Length; i++)
        {   
            Console.WriteLine("First index is " + castlingData[i]);
            if (castlingData.Length == 1 && castlingData[i] == '-')
            {
                Console.WriteLine("Both are false");
                K16 = false;
                Q16 = false;
                k32 = false;
                q32 = false;
             
            }
            
            else switch (castlingData[i])
            {
                case 'K':
                    K16 = true;
                    break;
                case 'Q':
                    Q16 = true;
                    break;
                case 'k':
                    k32 = true;
                    break;
                case 'q':
                    q32 = true;
                    break;
            }
        }
        
        ChessEngineSystem.Instance.UpdateTurnFromFen(turn);
        ChessEngineSystem.Instance.UpdateCastlingRightsFromFen(K16,Q16,k32,q32);
        
        
            
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
                int oldIndex = (8 * rank) + file; 
                
        #endregion
        
        #region newPosition
            int file1 = GetFileCode[code[2]];
            int rank1 = (int)char.GetNumericValue(code[3]) -1;
            int newIndex = (8 * rank1) + file1;
        #endregion
       // Console.WriteLine("old Index is =>" + oldIndex +", new  Index is => " +newIndex);

        return (oldIndex,newIndex);
    }


    public static string IndexToAlgebric(int oldIndex, int newIndex)
    {
        string files = "abcdefgh";
        string ranks = "12345678";

        int file_index_old = (oldIndex % 8);
        int rank_index_old = (oldIndex / 8) + 1;
        string algebraic_old = files[file_index_old].ToString() + ranks[rank_index_old - 1].ToString();

        int file_index_new = (newIndex % 8);
        int rank_index_new = (newIndex / 8) + 1;
        string algebraic_new = files[file_index_new].ToString() + ranks[rank_index_new - 1].ToString();

        string result = algebraic_old + algebraic_new;
        //Console.WriteLine(result);
        
        return result;
    }


}