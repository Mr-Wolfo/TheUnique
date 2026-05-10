using System.Collections.Generic;

public enum Direction
{
    Up, Down, Left, Right
}

public static class DirectionExtensions
{
    public static Direction Opposite(this Direction dir)
    {
        return dir switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => dir
        };
    }
}

public class PatternCompatibility
{
    private readonly Dictionary<Direction, Dictionary<int, HashSet<int>>> _rules = new();

    public void AddRule(int patternId, Direction dir, int compatibleId)
    {
        if (!_rules.ContainsKey(dir)) _rules[dir] = new Dictionary<int, HashSet<int>>();
        if (!_rules[dir].ContainsKey(patternId)) _rules[dir][patternId] = new HashSet<int>();
        
        _rules[dir][patternId].Add(compatibleId);
    }

    public HashSet<int> GetCompatible(int patternId, Direction dir)
    {
        if (_rules.TryGetValue(dir, out var dirRules) && dirRules.TryGetValue(patternId, out var set))
            return set;
        return new HashSet<int>();
    }
}