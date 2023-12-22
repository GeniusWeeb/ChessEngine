namespace ChessEngine;

public static class StockFishAnalysisResults
{
    public static int capturedPieceCount = 0;
    public static List<moveCells> moveCellsList = new List<moveCells>();

    public static void AddToResults(string name)
    {
        moveCells existingMove = moveCellsList.FirstOrDefault(move => String.Equals(name, move.name));

        if (existingMove != null)
        {
            // If the name already exists, increment the count
            existingMove.count += 1;
        }
        else
        {
            // If the name doesn't exist, add a new entry
            moveCellsList.Add(new moveCells(name, 1));
            Console.WriteLine("Added new entry");
        }
    }
}


public class moveCells
{
    public string name;
    public int count = 0;

    public moveCells(string name , int countName)
    {
        this.name = name;
       this. count +=  countName;
    }
}