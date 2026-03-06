#nullable disable
using System;
using System.Globalization;

namespace Microsoft.Maui.Controls.StyleSheets
{
	/// <summary>
	/// Resolves CSS value expressions including calc(), rem, and em units.
	/// Converts to device-independent pixels (the default MAUI unit).
	/// </summary>
	static class CssValueResolver
	{
		// Default root font size in device-independent pixels (matches browser default of 16px)
		internal static double DefaultRootFontSize = 16.0;

		/// <summary>
		/// Attempts to resolve a CSS value that may contain units (rem, em) or calc() expressions.
		/// Returns the resolved value string if resolution succeeded, or the original value if not applicable.
		/// </summary>
		public static string ResolveUnits(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			var trimmed = value.Trim();

			// Handle calc() expressions
			if (trimmed.StartsWith("calc(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(")", StringComparison.Ordinal))
			{
				var expr = trimmed.Substring(5, trimmed.Length - 6).Trim();
				if (TryEvaluateCalc(expr, out double result))
					return result.ToString(CultureInfo.InvariantCulture);
				return value; // Can't evaluate — leave for runtime
			}

			// Handle simple unit values: 1.5rem, 2em
			if (TryParseWithUnit(trimmed, out double resolved))
				return resolved.ToString(CultureInfo.InvariantCulture);

			return value;
		}

		static bool TryParseWithUnit(string value, out double result)
		{
			result = 0;

			if (value.EndsWith("rem", StringComparison.OrdinalIgnoreCase))
			{
				if (double.TryParse(value.Substring(0, value.Length - 3).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
				{
					result = num * DefaultRootFontSize;
					return true;
				}
			}
			else if (value.EndsWith("em", StringComparison.OrdinalIgnoreCase))
			{
				// em is relative to parent font size; approximate with root font size
				if (double.TryParse(value.Substring(0, value.Length - 2).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
				{
					result = num * DefaultRootFontSize;
					return true;
				}
			}
			else if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
			{
				// px maps 1:1 to device-independent pixels in MAUI
				if (double.TryParse(value.Substring(0, value.Length - 2).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
				{
					result = num;
					return true;
				}
			}

			return false;
		}

		static bool TryEvaluateCalc(string expr, out double result)
		{
			result = 0;
			// Simple expression evaluator: handles "A op B" where A/B can have units
			// Supports: +, -, *, /
			// Format: "value_with_unit operator value_with_unit"

			int opIndex = -1;
			char op = '\0';
			int parenDepth = 0;

			// Find the operator (+ or - at top level, not inside parens, not part of a number)
			// Search from the end to handle "1.5rem + 2" (the + after a space)
			for (int i = expr.Length - 1; i >= 1; i--)
			{
				char c = expr[i];
				if (c == ')')
					parenDepth++;
				else if (c == '(')
					parenDepth--;
				else if (parenDepth == 0 && (c == '+' || c == '-') && char.IsWhiteSpace(expr[i - 1]))
				{
					opIndex = i;
					op = c;
					break;
				}
			}

			// Try * and / if + and - not found
			if (opIndex < 0)
			{
				parenDepth = 0;
				for (int i = expr.Length - 1; i >= 1; i--)
				{
					char c = expr[i];
					if (c == ')')
						parenDepth++;
					else if (c == '(')
						parenDepth--;
					else if (parenDepth == 0 && (c == '*' || c == '/'))
					{
						opIndex = i;
						op = c;
						break;
					}
				}
			}

			if (opIndex < 0)
			{
				// No operator — try to resolve as a single value with unit
				return TryResolveValue(expr.Trim(), out result);
			}

			string leftStr = expr.Substring(0, opIndex).Trim();
			string rightStr = expr.Substring(opIndex + 1).Trim();

			if (!TryResolveValue(leftStr, out double left) || !TryResolveValue(rightStr, out double right))
				return false;

			switch (op)
			{
				case '+':
					result = left + right;
					return true;
				case '-':
					result = left - right;
					return true;
				case '*':
					result = left * right;
					return true;
				case '/':
					if (right == 0)
						return false;
					result = left / right;
					return true;
			}

			return false;
		}

		static bool TryResolveValue(string value, out double result)
		{
			result = 0;
			if (string.IsNullOrWhiteSpace(value))
				return false;

			// Try as unit value first
			if (TryParseWithUnit(value, out result))
				return true;

			// Try as plain number
			return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}
	}
}
