#nullable enable
namespace Microsoft.Maui
{
	internal static class StringExtensions
	{
		public static string? TrimToMaxLength(this string? currentText, int maxLength) =>
			maxLength >= 0 && currentText?.Length > maxLength
				? currentText.Substring(0, maxLength)
				: currentText;
	}
}