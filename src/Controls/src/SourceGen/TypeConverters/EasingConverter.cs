using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class EasingConverter : ISGTypeConverter
{
	static readonly HashSet<string> KnownEasingNames = new(StringComparer.OrdinalIgnoreCase)
	{
		"Linear", "SinOut", "SinIn", "SinInOut", "CubicIn", "CubicOut", "CubicInOut",
		"BounceOut", "BounceIn", "SpringIn", "SpringOut"
	};

	public IEnumerable<string> SupportedTypes => ["Easing", "Microsoft.Maui.Easing"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		var easingName = value;

		if (!string.IsNullOrWhiteSpace(easingName))
		{
			var easingType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Easing")!.ToFQDisplayString();

			if (TryConvertFunction(easingName.Trim(), easingType, out var call))
			{
				if (call is not null)
					return call;

				context.ReportConversionFailed(xmlLineInfo, value, Descriptors.EasingConversionFailed);
				return "default";
			}

			var parts = easingName.Split(['.']);
			if (parts.Length == 2 && parts[0].Equals("Easing", StringComparison.OrdinalIgnoreCase))
			{
				easingName = parts[1];
			}

			if (KnownEasingNames.Contains(easingName))
			{
				return $"{easingType}.{easingName}";
			}

			if (easingName.Equals("Hold", StringComparison.OrdinalIgnoreCase))
			{
				return $"{easingType}.Hold";
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.EasingConversionFailed);
		return "default";
	}

	// Validates the function forms at build time: Spring(), Spring(bounce), CubicBezier(x1, y1, x2, y2),
	// Steps(count) and Steps(count, start|end). Returns true with a null call for an invalid function.
	static bool TryConvertFunction(string text, string easingType, out string? call)
	{
		call = null;

		if (text.StartsWith("Easing.", StringComparison.OrdinalIgnoreCase))
			text = text.Substring("Easing.".Length);

		var open = text.IndexOf("(", StringComparison.Ordinal);
		if (open <= 0 || text[text.Length - 1] != ')')
			return false;

		var name = text.Substring(0, open).Trim();
		var argumentText = text.Substring(open + 1, text.Length - open - 2).Trim();
		var arguments = argumentText.Length == 0 ? Array.Empty<string>() : argumentText.Split(',');

		if (name.Equals("Spring", StringComparison.OrdinalIgnoreCase))
		{
			if (arguments.Length == 0)
				call = $"{easingType}.Spring()";
			else if (arguments.Length == 1 && TryParseNumber(arguments[0], out var bounce) && bounce >= 0 && bounce <= 0.95)
				call = $"{easingType}.Spring({Format(bounce)})";
		}
		else if (name.Equals("CubicBezier", StringComparison.OrdinalIgnoreCase))
		{
			if (arguments.Length == 4
				&& TryParseNumber(arguments[0], out var x1) && x1 >= 0 && x1 <= 1
				&& TryParseNumber(arguments[1], out var y1)
				&& TryParseNumber(arguments[2], out var x2) && x2 >= 0 && x2 <= 1
				&& TryParseNumber(arguments[3], out var y2))
				call = $"{easingType}.CubicBezier({Format(x1)}, {Format(y1)}, {Format(x2)}, {Format(y2)})";
		}
		else if (name.Equals("Steps", StringComparison.OrdinalIgnoreCase))
		{
			if (arguments.Length is 1 or 2
				&& int.TryParse(arguments[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) && count >= 1)
			{
				var position = arguments.Length == 2 ? arguments[1].Trim() : "end";
				if (position.Equals("start", StringComparison.OrdinalIgnoreCase) || position.Equals("jump-start", StringComparison.OrdinalIgnoreCase))
					call = $"{easingType}.Steps({count}, true)";
				else if (position.Equals("end", StringComparison.OrdinalIgnoreCase) || position.Equals("jump-end", StringComparison.OrdinalIgnoreCase))
					call = $"{easingType}.Steps({count}, false)";
			}
		}

		return true;

		static bool TryParseNumber(string text, out double number)
			=> double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out number)
				&& !double.IsNaN(number) && !double.IsInfinity(number);

		static string Format(double number) => number.ToString("R", CultureInfo.InvariantCulture) + "d";
	}
}
