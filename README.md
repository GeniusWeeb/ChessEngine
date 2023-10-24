# ChessEngine
A simple  neural chess Engine


#Chess board representation and rules :

1. Rules 
    King   : 2 , 
    Queen  : 2 ,
    Bishop : 4 ,
    Knight : 4 ,
    Rook   : 4 ,
    Pawn   : 16 


2. Cases -> En passant , 50 move rule (Draw) , castling , Promotion (Convert 1 peice to specific peice)


3. FEN (Forsyth Edward Notation)
   State - turn - castling_state(k/q-side) - EnPass_Square - plyCount(50 move rule counter) - FMC(Full move counter) 


   Notation:
        (For the GUI system )
        Black -  k , q , b , r , n , p
        white -  K , Q , B , R , N , P

