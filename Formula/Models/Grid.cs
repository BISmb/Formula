using System.Collections;
using System.Text.RegularExpressions;
using Formula.Expressions;

namespace Formula.Models;

public interface IGrid
{
    object GetValueForCell(GridCellReference cellReference);
    GridCellReference[] GetAllCellReferencesFromArray(CellReferenceArray cellArray);
}

public class Grid : IGrid
{
    public object GetValueForCell(GridCellReference cellReference)
    {
        return 2;
    }

    public GridCellReference[] GetAllCellReferencesFromArray(CellReferenceArray cellArray)
    {
        throw new NotImplementedException();
    }

    public static IEnumerator<GridCellReference> GetCellNamesBetweenPositions(GridCellReference start, GridCellReference end)
    {
        // todo: assumed 1 dimensional (skip 1st column as different)

        string posY = GetYString(start);
        
        int startX = GetXNumber(start);
        int endX = GetXNumber(end);
        
        for (int xPos = startX; xPos <= endX; xPos++) // inclusive
        {
            GridCellReference newRef = new(string.Concat(posY, xPos));
            yield return newRef;
        }
    }

    private static string GetYString(GridCellReference cellReference)
    {
        string pattern = @"[a-zA-Z]+";
        
        Match match = Regex.Match($"{cellReference}", pattern);
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid cell reference: {cellReference}");
        }
        
        return match.Value;
    }

    private static int GetXNumber(GridCellReference cellReference)
    {
        string pattern = @"\d+";
        
        Match match = Regex.Match($"{cellReference}", pattern);
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid cell reference: {cellReference}");
        }
        
        return int.Parse(match.Value);
    }
}

public struct GridCellReference(string Position)
{
    public override string ToString()
    {
        return Position;
    }
};
    
public struct CellReferenceArray : IEnumerable<GridCellReference>
{
    public GridCellReference Start { get; init; }
    public GridCellReference End { get; init; }

    public CellReferenceArray(GridCellReference start, GridCellReference end)
    {
        Start = start;
        End = end;
    }

    public CellReferenceArray(string arrayDefinedInString)
    {
        string[] posCells = arrayDefinedInString.Split(':', StringSplitOptions.TrimEntries);

        if (posCells.Length != 2)
        {
            throw new ArgumentException($"Invalid cell reference: {arrayDefinedInString}");
        }
        
        Start = new(posCells[0]);
        End = new(posCells[1]);
    }

    public IEnumerator<GridCellReference> GetEnumerator()
    {
        return Grid.GetCellNamesBetweenPositions(Start, End);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}