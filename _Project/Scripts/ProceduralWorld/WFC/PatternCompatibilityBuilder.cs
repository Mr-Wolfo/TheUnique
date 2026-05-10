using System.Collections.Generic;

public static class PatternCompatibilityBuilder
{
    public static PatternCompatibility Build(PatternLibrarySO library)
    {
        var comp = new PatternCompatibility();
        foreach (var rule in library.Adjacencies)
        {
            comp.AddRule(rule.FromId, rule.Direction, rule.ToId);
        }
        return comp;
    }
}