namespace Microsoft.Maui.Controls.BindingSourceGen;

public sealed record Setter(string[] PatternMatchingExpressions, string AssignmentStatement)
{
    public static Setter From(
        IEnumerable<IPathPart> path,
        uint id,
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

                accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, innerPart, id: id);
            }
            else
            {
                accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, part, id, part == path.Last());
            }
        }

        return new Setter(
            patternMatchingExpressions.ToArray(),
            AssignmentStatement: BuildAssignmentStatement(accessAccumulator, path.Any() ? path.Last() : null, id, assignedValueExpression));
    }

    public static string BuildAssignmentStatement(string accessAccumulator, IPathPart? lastPart, uint id, string assignedValueExpression = "value") =>
        lastPart switch
        {
            InaccessibleMemberAccess inaccessibleMemberAccess when inaccessibleMemberAccess.Kind == AccessorKind.Property => $"SetUnsafeProperty{id}{inaccessibleMemberAccess.MemberName}({accessAccumulator}, {assignedValueExpression});",
            _ => $"{accessAccumulator} = {assignedValueExpression};",
        };
}
