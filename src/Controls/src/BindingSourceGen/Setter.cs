namespace Microsoft.Maui.Controls.BindingSourceGen;

public sealed record Setter(string[] PatternMatchingExpressions, string AssignmentStatement)
{
    public static Setter From(
        IEnumerable<IPathPart> path,
        string sourceVariableName = "source",
        string assignedValueExpression = "value")
    {
        string accessAccumulator = sourceVariableName;
        List<string> patternMatchingExpressions = new();
        bool skipNextConditionalAccess = false;

        void AddPatternMatchingExpression(string pattern)
        {
            var tmpVariableName = $"p{patternMatchingExpressions.Count}";
            patternMatchingExpressions.Add($"{accessAccumulator} is {pattern} {tmpVariableName}");
            accessAccumulator = tmpVariableName;
        }

        foreach (var part in path)
        {
            var skipConditionalAccess = skipNextConditionalAccess;
            skipNextConditionalAccess = false;
            
            if (part is Cast { TargetType: var targetType })
            {
                AddPatternMatchingExpression(targetType.GlobalName);

                // the current `is T` expression makes sure that the value is not null
                // so if a conditional access to a member/indexer follows, we can skip the next null check
                skipNextConditionalAccess = true;
            }
            else if (part is ConditionalAccess { Part: var innerPart })
            {
                if (!skipConditionalAccess)
                {
                    AddPatternMatchingExpression("{}");
                }

                accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, innerPart);
            }
            else
            {
                accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, part);
            }
        }

        return new Setter(
            patternMatchingExpressions.ToArray(),
            AssignmentStatement: $"{accessAccumulator} = {assignedValueExpression};");
    }
}
