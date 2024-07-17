namespace Microsoft.Maui.Controls.BindingSourceGen; 

public static class AccessExpressionBuilder
{
    public static string ExtendExpression(string previousExpression, IPathPart nextPart)
        => nextPart switch
        {
            Cast { TargetType: var targetType } => $"({previousExpression} as {CastTargetName(targetType)})",
            ConditionalAccess conditionalAccess => ExtendExpression(previousExpression: $"{previousExpression}?", conditionalAccess.Part),
            IndexAccess { Index: int numericIndex } => $"{previousExpression}[{numericIndex}]",
            IndexAccess { Index: string stringIndex } => $"{previousExpression}[\"{stringIndex}\"]",
            MemberAccess memberAccess => $"{previousExpression}.{memberAccess.MemberName}",
            _ => throw new NotSupportedException($"Unsupported path part type: {nextPart.GetType()}"),
        };

    private static string CastTargetName(TypeDescription targetType)
        => targetType.IsValueType ? $"{targetType.GlobalName}?" : targetType.GlobalName;
}
