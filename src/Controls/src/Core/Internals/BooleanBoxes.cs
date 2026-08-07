using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Internals;

internal static class BooleanBoxes
{
	internal static readonly object TrueBox = true;
	internal static readonly object FalseBox = false;

	internal static object Box(bool value) =>
		value ? TrueBox : FalseBox;

	[return: NotNullIfNotNull(nameof(value))]
	internal static object? Box(bool? value) =>
		value switch
		{
			true => TrueBox,
			false => FalseBox,
			null => null,
		};
}
