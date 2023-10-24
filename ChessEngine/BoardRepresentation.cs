
namespace ChessEngine {


// NOTE  :  This will  Handle BOARD CREATION 

 public class BoardRepresentation {

   public void CreateChessBoard(string name = "zim")
    {
         Console.WriteLine( name +" is Creating chess board \n\n\n\n");
         int count = 0;

         for (int i = 0  ; i < 8; i++ )
         {
            for (int j =0 ; j <  8 ; j++)
                {
                    Console.Write(count + " ");
                    count++;
                }
            Console.Write("\n");

         }

    }

 }   


}
