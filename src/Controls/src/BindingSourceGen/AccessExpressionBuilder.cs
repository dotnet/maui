namespace Microsoft.Maui.Controls.BindingSourceGen;

using static Microsoft.Maui.Controls.BindingSourceGen.UnsafeAccessorsMethodName;


public static class AccessExpressionBuilder
{
	/// <summary>
	/// Extends an expression by accessing the next part of a binding path.
	/// This is used for getter paths (reading values), not for setter assignments.
	/// </summary>
	public static string ExtendExpression(string previousExpression, IPathPart nextPart)
		=> nextPart switch
		{
			Cast { TargetType: var targetType } => $"({previousExpression} as {CastTargetName(targetType)})",
			ConditionalAccess conditionalAccess => ExtendExpression(previousExpression: $"{previousExpression}?", conditionalAccess.Part),
			IndexAccess { Index: int numericIndex } => $"{previousExpression}[{numericIndex}]",
			IndexAccess { Index: string stringIndex } => $"{previousExpression}[\"{stringIndex}\"]",
			MemberAccess memberAccess => $"{previousExpression}.{memberAccess.MemberName}",
			InaccessibleMemberAccess inaccessibleMemberAccess when inaccessibleMemberAccess.Kind == AccessorKind.Field => $"{CreateUnsafeFieldAccessorMethodName(inaccessibleMemberAccess.MemberName)}({previousExpression})",
			InaccessibleMemberAccess inaccessibleMemberAccess when inaccessibleMemberAccess.Kind == AccessorKind.Property && inaccessibleMemberAccess.IsGetterInaccessible => $"{CreateUnsafePropertyAccessorGetMethodName(inaccessibleMemberAccess.MemberName)}({previousExpression})",
			InaccessibleMemberAccess inaccessibleMemberAccess when inaccessibleMemberAccess.Kind == AccessorKind.Property => $"{previousExpression}.{inaccessibleMemberAccess.MemberName}",
			_ => throw new NotSupportedException($"Unsupported path part type: {nextPart.GetType()}"),
		};

	private static string CastTargetName(TypeDescription targetType)
		=> targetType.IsValueType ? $"{targetType.GlobalName}?" : targetType.GlobalName;
}
