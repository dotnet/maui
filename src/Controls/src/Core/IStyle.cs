#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	interface IStyle
	{
		Type TargetType
		{
			[RequiresUnreferencedCode("TargetType may have been trimmed when using lazy styles.")]
			get;
		}

		void Apply(BindableObject bindable, SetterSpecificity specificity);
		void UnApply(BindableObject bindable);
	}

	internal static class IStyleExtensions
	{
		/// <summary>
		/// Attempts to get the target type from an IStyle. Returns false if the type was trimmed.
		/// </summary>
		[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
			Justification = "We expect types to be trimmed in Release builds. Style uses Type.GetType which returns null for trimmed types. Non-Style IStyle implementations are expected to have stable TargetType.")]
		internal static bool TryGetTargetType(this IStyle style, [NotNullWhen(true)] out Type targetType)
		{
			targetType = style.TargetType;
			return targetType is not null;
		}
	}
}