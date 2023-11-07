namespace ChessEngine;

public static  class Piece
{
    // Binary representation of the numbers here 
    // Aim is to make computation easy 
    // We could also go for 2d matrix but that may not be needed:w

    
    // 32 16  - 8 4 2 0
    // color    pieces                
    public const  int Empty = 0;
    public const int Pawn= 1; // 0001
    public const int Rook= 2; // 0010 
    public const int Knight= 3; // 0011
    public const int Bishop= 4; // 0100
    
    public const int King = 8; //1000K
    public const int Queen = 6; // 0110


    public const int Black = 32; // 10 -0000
    public const int White = 16; // 01 -0000
    
    
    //Mask for PIECES

    public const int CPiece = 15;

}

