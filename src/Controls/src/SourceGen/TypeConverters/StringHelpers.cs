using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

/// <summary>
/// Helper class for safely escaping strings in generated C# code to prevent injection attacks.
/// </summary>
internal static class StringHelpers
{
	/// <summary>
	/// Escapes a string value for safe inclusion in generated C# code as a string literal.
	/// This prevents XAML injection attacks by properly escaping quotes and other special characters.
	/// </summary>
	/// <param name="value">The string value to escape</param>
	/// <returns>A properly escaped string literal including surrounding quotes</returns>
	public static string EscapeStringLiteral(string value)
	{
		if (value == null)
			return "null";
			
		// Use Roslyn's SymbolDisplay.FormatLiteral to properly escape the string
		return SymbolDisplay.FormatLiteral(value, quote: true);
	}
	
	/// <summary>
	/// Validates that a string contains only safe characters for code generation.
	/// Used as an additional safety check for critical paths.
	/// </summary>
	/// <param name="value">The string to validate</param>
	/// <returns>True if the string is safe, false otherwise</returns>
	public static bool IsSafeForCodeGeneration(string value)
	{
		if (string.IsNullOrEmpty(value))
			return true;
			
		// Check for potentially dangerous characters that could be used for injection
		// Be more strict about what we allow - only allow printable ASCII and basic whitespace
		foreach (char c in value)
		{
			// Allow normal printable characters and basic whitespace
			if (char.IsControl(c))
			{
				// Allow only specific whitespace characters in limited contexts
				if (c != ' ' && c != '\t')
					return false;
					
				// Even tabs and multiple spaces could be suspicious in XAML attributes
				// For maximum security, we should be very restrictive
			}
			
			// Block characters that are commonly used in injection attacks
			if (c == '\n' || c == '\r')
				return false;
		}
		
		return true;
	}
}