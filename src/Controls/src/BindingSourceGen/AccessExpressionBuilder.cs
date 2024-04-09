using System;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Controls.BindingSourceGen
{
    public static class AccessExpressionBuilder
    {
        public static string Build(string previousExpression, IPathPart nextPart)
            => nextPart switch
            {
                Cast { TargetType: var targetType } => $"({previousExpression} as {CastTargetName(targetType)})",
                ConditionalAccess conditionalAccess => Build(previousExpression: $"{previousExpression}?", conditionalAccess.Part),
                IndexAccess indexer => $"{previousExpression}[{indexer.Index.FormattedIndex}]",
                MemberAccess memberAccess => $"{previousExpression}.{memberAccess.MemberName}",
                _ => throw new NotSupportedException($"Unsupported path part type: {nextPart.GetType()}"),
            };

        private static string CastTargetName(TypeDescription targetType)
            => targetType.IsValueType ? $"{targetType.GlobalName}?" : targetType.GlobalName;
    }
}
