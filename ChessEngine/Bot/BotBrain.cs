using Utility;
using Tensorflow;
namespace ChessEngine.Bot;

// this will be responsible to make decisions on the bot side
public class BotBrain
{

    public bool isSmart;

   public  BotBrain(bool supremeBot)
   {
   
       this.isSmart = supremeBot;
   }

    public void Think(Board board )
    {
        ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor("Bot is starting to think");
        Move move = ChessEngineSystem.Instance.miniMax.FindBestMove(board, isSmart, 1); 
        Console.WriteLine("Thinking finished");// comes while creating the bot
        BotMakeFinalMove(move);

    }


    
    
    //Integrated with MiniMax Algo -> maybe can add Alpha beta pruning
    private void BotMakeFinalMove(Move move)
    {
        Console.WriteLine("Chosen new index is " + move.to);
        if (ChessEngineSystem.Instance.GetBoardClass.BotMakeMove(move))
        {
            //board doesnt need to send validate to the ui  , since it is authoritative
            ChessEngineSystem.Instance.UpdateUIWithNewIndex(move.from, move.to);
        }
    }



}