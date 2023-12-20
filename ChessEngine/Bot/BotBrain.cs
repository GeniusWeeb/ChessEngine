using Utility;

namespace ChessEngine.Bot;

// this will be responsible to make decisions on the bot side
public class BotBrain
{

    public void Think()
    {
        #region Obsolete -> Takes random moves from the unfilterd move list and forces it on the make move 
        
             Random random = new Random();
        
        int randomIndex = random.Next(0, GameStateManager.Instance.allPiecesThatCanMove.Count);
        ChessPiece  piece=GameStateManager.Instance.allPiecesThatCanMove[randomIndex];
            
        //------------------------------------------------------------
        int oldIndex = piece.GetCurrentIndex;
        int newRandomIndex = random.Next(0, piece.GetAllMovesForThisPiece.Count);
        
        //------------------------------------------------
        List<int> tempList = piece.GetAllMovesForThisPiece.ToList();
        int  newIndex = tempList[newRandomIndex];
        
        #endregion
            
        //Own pieces moves
        foreach (var Cp in GameStateManager.Instance.allPiecesThatCanMove)
        {

            foreach (var moves in Cp.allPossibleMovesIndex)

            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"MOVES ARE {Cp.GetPieceCode} To {moves}");
                Console.ResetColor();

            }
        }
        
        
        //------------------------------------------------------------
        if (ChessEngineSystem.Instance.GetBoardClass.MakeMove(oldIndex, newIndex ))
        {
            //board doesnt need to send validate to the ui  , since it is authoritative
             ChessEngineSystem.Instance. UpdateUIWithNewIndex(oldIndex, newIndex);
            

             
             
             //Note : I think the bot does not have the ALLOWED MOVE LIST 
             //AFTER KING PRE CHECK TEST CALCULATION
        }

      
    }



}