using Utility;

namespace ChessEngine.Bot;

// this will be responsible to make decisions on the bot side
public class BotBrain
{

    public void Think( ref List<ChessPiece> allPiecesThatCanMove)
    {
        Random random = new Random();

        int randomIndex = random.Next(0, allPiecesThatCanMove.Count);
        ChessPiece  piece= allPiecesThatCanMove[randomIndex];
            
        //------------------------------------------------------------
        int oldIndex = piece.GetCurrentIndex;
        int newRandomIndex = random.Next(0, piece.GetAllMovesForThisPiece.Count);
        
        //------------------------------------------------
        List<int> tempList = piece.GetAllMovesForThisPiece.ToList();
        int  newIndex = tempList[newRandomIndex];
        
        //------------------------------------------------------------
        if (ChessEngineSystem.Instance.getBoard.MakeMove(piece.GetPieceCode, oldIndex, newIndex))
        {
            
          //  Protocols finalData = new Protocols(ProtocolTypes.VALIDATE.ToString() ,"true" , GameStateManager.Instance.GetTurnToMove.ToString());
            //ChessEngineSystem.Instance. SendDataToUI(finalData);
            ChessEngineSystem.Instance. UpdateUIWithNewIndex(oldIndex, newIndex);
            ChessEngineSystem.Instance. ScanBoardForMoves();
        }
    }



}