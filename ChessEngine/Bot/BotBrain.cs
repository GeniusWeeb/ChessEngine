using Utility;

namespace ChessEngine.Bot;

// this will be responsible to make decisions on the bot side
public class BotBrain
{ 
    Random random = new Random();

    public void Think()
    {
        
        ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor("Bot is starting to think");
        
        foreach (var p in GameStateManager.Instance.allPiecesThatCanMove)
        {
            foreach (var mo in p.allPossibleMovesIndex)
            {
                
                Console.WriteLine($"{p.GetPieceCode} can move to {mo}");
            }
        }
        
        Console.WriteLine($"All Pieces that can move are  {GameStateManager.Instance.allPiecesThatCanMove.Count}" );
        
            
        //Exception when its just 1 piece or just 1 move
 
       
        ChessPiece ChosenPiece;


        if (GameStateManager.Instance.allPiecesThatCanMove.Count == 0)  ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor("I lost }", ConsoleColor.Yellow);
        else if (GameStateManager.Instance.allPiecesThatCanMove.Count ==1) ChosenPieceMoves(GameStateManager.Instance.allPiecesThatCanMove.ToList()[0]);
          else if (GameStateManager.Instance.allPiecesThatCanMove.Count > 1)
        {
            int randomPieceIndex =
                random.Next(0,
                    GameStateManager.Instance.allPiecesThatCanMove.Count); // //Whole List contains more than 1 Piece

            ChosenPiece = GameStateManager.Instance.allPiecesThatCanMove.ToList()[randomPieceIndex];
            ChosenPieceMoves(ChosenPiece);
        }


        // #region Obsolete -> Takes random moves from the unfilterd move list and forces it on the make move
        //
        //     
        //     int randomIndex = random.Next(0, GameStateManager.Instance.allPiecesThatCanMove.Count);
        //     ChessPiece  piece=GameStateManager.Instance.allPiecesThatCanMove[randomIndex];
        //         
        //     //------------------------------------------------------------
        //     int oldIndex = piece.GetCurrentIndex;
        //     int newRandomIndex = random.Next(0, piece.GetAllMovesForThisPiece.Count);
        //     ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor($"Bot has decided new index to be {newRandomIndex}");
        //
        //     //------------------------------------------------
        //     try
        //     {
        //         foreach (var move in piece.GetAllMovesForThisPiece)
        //         {
        //             Console.WriteLine(move);
        //         }
        //         
        //         
        //         Console.WriteLine("SHOWING ALL LEGAL MOVES FROM WHICH NEW INDEX COMING OUT");
        //         List<int> tempList = piece.GetAllMovesForThisPiece.ToList();
        //         
        //
        //         int newIndex = tempList[newRandomIndex];
        //         
        //         ChessEngineSystem.Instance.UtilityWriteToConsoleWithColor("Final index decided");
        //         
        //         int count = 0;
        //         
        //
        //         //Own pieces moves
        //         foreach (var Cp in GameStateManager.Instance.allPiecesThatCanMove)
        //         {
        //
        //             foreach (var moves in Cp.allPossibleMovesIndex)
        //
        //             {
        //                 // Console.ForegroundColor = ConsoleColor.Blue;
        //                 // Console.WriteLine($"MOVES ARE {Cp.GetPieceCode} To {moves}");
        //                 count += 1;
        //                 // Console.ResetColor();
        //
        //             }
        //         }
        //
        //         Console.WriteLine($"Total available moves to this bot are {count}");
        //         Console.WriteLine($"Chosen move index is {piece.GetPieceCode} -->{oldIndex} to {newIndex}");
        //
        //
        //
        //
        //         //------------------------------------------------------------
        //         
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         throw;
        //     }
        // #endregion
    }

    private void ChosenPieceMoves(ChessPiece ChosenPiece)
    {
        Console.WriteLine($"Chosen Piece is {ChosenPiece.GetCurrentIndex}");
        // if the whole list contains just 1 piece
        // if (ChosenPiece.allPossibleMovesIndex.Count == 0)
        // {
        //     Console.WriteLine("This Piece has no moves");
        //     return;
        // } 
        if (ChosenPiece.allPossibleMovesIndex.Count == 1) // 1 Piece also has 1 move left
        {
            foreach (var m in ChosenPiece.allPossibleMovesIndex )
            {
                Console.WriteLine(m);
            }
            BotFinalMove(ChosenPiece.GetCurrentIndex, ChosenPiece.allPossibleMovesIndex.Single());
                   
        }
        else if (ChosenPiece.allPossibleMovesIndex.Count > 1 ) // 1 Piece  has many moves left
        {   
            foreach (var m in ChosenPiece.allPossibleMovesIndex )
            {
                Console.WriteLine(m);
            }

            var rand = random.Next(0, ChosenPiece.allPossibleMovesIndex.Count);
            var tempList = ChosenPiece.allPossibleMovesIndex.ToList();
            if(tempList.Count != 0)
                BotFinalMove(ChosenPiece.GetCurrentIndex , tempList[rand] );
        }
        
    }

    private void BotFinalMove(int oldIndex, int newIndex)
    {
        Console.WriteLine("Chosen new index is " + newIndex);
        if (ChessEngineSystem.Instance.GetBoardClass.MakeMove(oldIndex, newIndex))
        {
            //board doesnt need to send validate to the ui  , since it is authoritative
            ChessEngineSystem.Instance.UpdateUIWithNewIndex(oldIndex, newIndex);


            //Note : I think the bot does not have the ALLOWED MOVE LIST 
            //AFTER KING PRE CHECK TEST CALCULATION
        }
    }



}