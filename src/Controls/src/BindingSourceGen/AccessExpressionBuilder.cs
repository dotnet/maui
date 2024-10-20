namespace Microsoft.Maui.Controls.BindingSourceGen;
using static Microsoft.Maui.Controls.BindingSourceGen.UnsafeAccessorsMethodName;


public static class AccessExpressionBuilder
{
	public static string ExtendExpression(string previousExpression, IPathPart nextPart, uint bindingId, bool shouldUseUnsafePropertySetter = false)
		=> nextPart switch
		{
			Cast { TargetType: var targetType } => $"({previousExpression} as {CastTargetName(targetType)})",
			ConditionalAccess conditionalAccess => ExtendExpression(previousExpression: $"{previousExpression}?", conditionalAccess.Part, bindingId: bindingId),
			IndexAccess { Index: int numericIndex } => $"{previousExpression}[{numericIndex}]",
			IndexAccess { Index: string stringIndex } => $"{previousExpression}[\"{stringIndex}\"]",
			MemberAccess memberAccess => $"{previousExpression}.{memberAccess.MemberName}",
			InaccessibleMemberAccess inaccessibleMemberAccess when inaccessibleMemberAccess.Kind == AccessorKind.Field => $"{CreateUnsafeFieldAccessorMethodName(bindingId, inaccessibleMemberAccess.MemberName)}({previousExpression})",
			InaccessibleMemberAccess inaccessibleMemberAccess when inaccessibleMemberAccess.Kind == AccessorKind.Property && !shouldUseUnsafePropertySetter => $"{CreateUnsafePropertyAccessorGetMethodName(bindingId, inaccessibleMemberAccess.MemberName)}({previousExpression})",
			InaccessibleMemberAccess inaccessibleMemberAccess when inaccessibleMemberAccess.Kind == AccessorKind.Property => previousExpression, // This case is handled by the caller
			_ => throw new NotSupportedException($"Unsupported path part type: {nextPart.GetType()}"),
		};

	private static string CastTargetName(TypeDescription targetType)
		=> targetType.IsValueType ? $"{targetType.GlobalName}?" : targetType.GlobalName;
}
