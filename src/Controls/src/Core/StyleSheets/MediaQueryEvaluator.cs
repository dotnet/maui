#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls.StyleSheets
{
	/// <summary>
	/// Evaluates CSS @media conditions against the current window/app state.
	/// Supports: min-width, max-width, min-height, max-height, orientation, prefers-color-scheme.
	/// </summary>
	internal static class MediaQueryEvaluator
	{
		/// <summary>
		/// Evaluates whether a media condition string is satisfied given the current context.
		/// </summary>
		/// <param name="condition">The media condition text, e.g. "(min-width: 768px)" or "screen and (min-width: 768px)".</param>
		/// <param name="widthDip">Window width in device-independent pixels.</param>
		/// <param name="heightDip">Window height in device-independent pixels.</param>
		/// <param name="appTheme">Current application theme.</param>
		/// <returns>True if the condition matches.</returns>
		public static bool Evaluate(string condition, double widthDip, double heightDip, AppTheme appTheme)
		{
			if (string.IsNullOrWhiteSpace(condition))
				return false;

			// Handle "and"-separated conditions: "screen and (min-width: 768px) and (max-width: 1200px)"
			// Split on " and " and evaluate each part
			var parts = SplitOnAnd(condition);
			foreach (var part in parts)
			{
				var trimmed = part.Trim();

				// Skip media type keywords: "screen", "all", "print" — we match screen/all
				if (IsMediaType(trimmed))
					continue;

				if (!EvaluateSingle(trimmed, widthDip, heightDip, appTheme))
					return false;
			}

			return true;
		}

		static bool EvaluateSingle(string condition, double widthDip, double heightDip, AppTheme appTheme)
		{
			// Strip outer parens: "(min-width: 768px)" → "min-width: 768px"
			var expr = condition.Trim();
			if (expr.StartsWith("(", StringComparison.Ordinal) && expr.EndsWith(")", StringComparison.Ordinal))
				expr = expr.Substring(1, expr.Length - 2).Trim();

			int colonIdx = expr.IndexOf(':', StringComparison.Ordinal);
			if (colonIdx < 0)
				return false;

			string feature = expr.Substring(0, colonIdx).Trim().ToLowerInvariant();
			string valueStr = expr.Substring(colonIdx + 1).Trim();

			switch (feature)
			{
				case "min-width":
					return TryParseLength(valueStr, out double minW) && widthDip >= minW;
				case "max-width":
					return TryParseLength(valueStr, out double maxW) && widthDip <= maxW;
				case "min-height":
					return TryParseLength(valueStr, out double minH) && heightDip >= minH;
				case "max-height":
					return TryParseLength(valueStr, out double maxH) && heightDip <= maxH;
				case "orientation":
					if (string.Equals(valueStr, "portrait", StringComparison.OrdinalIgnoreCase))
						return heightDip >= widthDip;
					if (string.Equals(valueStr, "landscape", StringComparison.OrdinalIgnoreCase))
						return widthDip > heightDip;
					return false;
				case "prefers-color-scheme":
					if (string.Equals(valueStr, "dark", StringComparison.OrdinalIgnoreCase))
						return appTheme == AppTheme.Dark;
					if (string.Equals(valueStr, "light", StringComparison.OrdinalIgnoreCase))
						return appTheme == AppTheme.Light;
					return false;
				default:
					return false; // Unknown feature — skip gracefully
			}
		}

		static bool TryParseLength(string value, out double result)
		{
			result = 0;
			if (string.IsNullOrEmpty(value))
				return false;

			// Strip unit suffixes (px, rem, em) — convert to DIP
			if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
				value = value.Substring(0, value.Length - 2).Trim();
			else if (value.EndsWith("rem", StringComparison.OrdinalIgnoreCase))
			{
				value = value.Substring(0, value.Length - 3).Trim();
				if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				{
					result *= 16.0; // 1rem = 16 DIP
					return true;
				}
				return false;
			}
			else if (value.EndsWith("em", StringComparison.OrdinalIgnoreCase))
			{
				value = value.Substring(0, value.Length - 2).Trim();
				if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				{
					result *= 16.0;
					return true;
				}
				return false;
			}

			return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}

		static List<string> SplitOnAnd(string condition)
		{
			var parts = new List<string>();
			int start = 0;
			int depth = 0;

			for (int i = 0; i < condition.Length; i++)
			{
				char c = condition[i];
				if (c == '(') depth++;
				else if (c == ')') depth--;
				else if (depth == 0 && i + 5 <= condition.Length
					&& condition.Substring(i, 5).Equals(" and ", StringComparison.OrdinalIgnoreCase))
				{
					parts.Add(condition.Substring(start, i - start));
					start = i + 5;
					i += 4; // skip past " and"
				}
			}

			if (start < condition.Length)
				parts.Add(condition.Substring(start));

			return parts;
		}

		static bool IsMediaType(string value)
		{
			return string.Equals(value, "screen", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(value, "all", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(value, "print", StringComparison.OrdinalIgnoreCase);
		}
	}
}
