namespace ChessEngine;

public static  class Piece
{
    // Binary representation of the numbers here 
    // Aim is to make computation easy 
    // We could also go for 2d matrix but that may not be needed:w

    
    // 32 16  - 8 4 2 0
    // color    pieces                
    public static  int Empty = 0;
    public static int Pawn= 1; // 0001
    public static int Rook= 2; // 0010 
    public static int Knight= 3; // 0011
    public static int Bishop= 4; // 0100
    
    public static int King = 8; //1000K
    public static int Queen = 6; // 0110


    public static int Black = 32; // 10 -0000
    public static int White = 16; // 01 -0000
    
    
    //Mask for PIECES

    public static int CPiece = 15;

}