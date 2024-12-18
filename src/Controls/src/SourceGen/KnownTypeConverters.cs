using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;

static class KnownTypeConverters
{
    static readonly HashSet<string> KnownNamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "AliceBlue",
        "AntiqueWhite",
        "Aqua",
        "Aquamarine",
        "Azure",
        "Beige",
        "Bisque",
        "Black",
        "BlanchedAlmond",
        "Blue",
        "BlueViolet",
        "Brown",
        "BurlyWood",
        "CadetBlue",
        "Chartreuse",
        "Chocolate",
        "Coral",
        "CornflowerBlue",
        "Cornsilk",
        "Crimson",
        "Cyan",
        "DarkBlue",
        "DarkCyan",
        "DarkGoldenrod",
        "DarkGray",
        "DarkGreen",
        "DarkGrey",
        "DarkKhaki",
        "DarkMagenta",
        "DarkOliveGreen",
        "DarkOrange",
        "DarkOrchid",
        "DarkRed",
        "DarkSalmon",
        "DarkSeaGreen",
        "DarkSlateBlue",
        "DarkSlateGray",
        "DarkSlateGrey",
        "DarkTurquoise",
        "DarkViolet",
        "DeepPink",
        "DeepSkyBlue",
        "DimGray",
        "DimGrey",
        "DodgerBlue",
        "Firebrick",
        "FloralWhite",
        "ForestGreen",
        "Fuchsia",
        "Gainsboro",
        "GhostWhite",
        "Gold",
        "Goldenrod",
        "Gray",
        "Green",
        "GreenYellow",
        "Grey",
        "Honeydew",
        "HotPink",
        "IndianRed",
        "Indigo",
        "Ivory",
        "Khaki",
        "Lavender",
        "LavenderBlush",
        "LawnGreen",
        "LemonChiffon",
        "LightBlue",
        "LightCoral",
        "LightCyan",
        "LightGoldenrodYellow",
        "LightGray",
        "LightGreen",
        "LightGrey",
        "LightPink",
        "LightSalmon",
        "LightSeaGreen",
        "LightSkyBlue",
        "LightSlateGray",
        "LightSlateGrey",
        "LightSteelBlue",
        "LightYellow",
        "Lime",
        "LimeGreen",
        "Linen",
        "Magenta",
        "Maroon",
        "MediumAquamarine",
        "MediumBlue",
        "MediumOrchid",
        "MediumPurple",
        "MediumSeaGreen",
        "MediumSlateBlue",
        "MediumSpringGreen",
        "MediumTurquoise",
        "MediumVioletRed",
        "MidnightBlue",
        "MintCream",
        "MistyRose",
        "Moccasin",
        "NavajoWhite",
        "Navy",
        "OldLace",
        "Olive",
        "OliveDrab",
        "Orange",
        "OrangeRed",
        "Orchid",
        "PaleGoldenrod",
        "PaleGreen",
        "PaleTurquoise",
        "PaleVioletRed",
        "PapayaWhip",
        "PeachPuff",
        "Peru",
        "Pink",
        "Plum",
        "PowderBlue",
        "Purple",
        "Red",
        "RosyBrown",
        "RoyalBlue",
        "SaddleBrown",
        "Salmon",
        "SandyBrown",
        "SeaGreen",
        "SeaShell",
        "Sienna",
        "Silver",
        "SkyBlue",
        "SlateBlue",
        "SlateGray",
        "SlateGrey",
        "Snow",
        "SpringGreen",
        "SteelBlue",
        "Tan",
        "Teal",
        "Thistle",
        "Tomato",
        "Transparent",
        "Turquoise",
        "Violet",
        "Wheat",
        "White",
        "WhiteSmoke",
        "Yellow",
        "YellowGreen"
    };

    // #rgb, #rrggbb, #aarrggbb are all valid 
    const string RxColorHexPattern = @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}([0-9a-fA-F]{2})?)$";
    static readonly Lazy<Regex> RxColorHex = new(() => new Regex(RxColorHexPattern, RegexOptions.Compiled | RegexOptions.Singleline));

    // RGB		rgb(255,0,0), rgb(100%,0%,0%)					values in range 0-255 or 0%-100%
    // RGBA		rgba(255, 0, 0, 0.8), rgba(100%, 0%, 0%, 0.8)	opacity is 0.0-1.0
    // HSL		hsl(120, 100%, 50%)								h is 0-360, s and l are 0%-100%
    // HSLA		hsla(120, 100%, 50%, .8)						opacity is 0.0-1.0
    // HSV		hsv(120, 100%, 50%)								h is 0-360, s and v are 0%-100%
    // HSVA		hsva(120, 100%, 50%, .8)						opacity is 0.0-1.0
    const string RxFuncPattern = "^(?<func>rgba|argb|rgb|hsla|hsl|hsva|hsv)\\(((?<v>\\d%?),){2}((?<v>\\d%?)|(?<v>\\d%?),(?<v>\\d%?))\\);?$";
    static readonly Lazy<Regex> RxFuncExpr = new(() => new Regex(RxFuncPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline));

    static readonly HashSet<string> KnownEasingNames = new (StringComparer.OrdinalIgnoreCase)
    {
        "Linear",
        "SinOut",
        "SinIn",
        "SinInOut",
        "CubicIn",
        "CubicOut",
        "CubicInOut",
        "BounceOut",
        "BounceIn",
        "SpringIn",
        "SpringOut"
    };

    const string Ellipse = nameof(Ellipse);
    const string Line = nameof(Line);
    const string Path = nameof(Path);
    const string Polygon = nameof(Polygon);
    const string Polyline = nameof(Polyline);
    const string Rectangle = nameof(Rectangle);
    const string RoundRectangle = nameof(RoundRectangle);
    internal static readonly char[] Delimiter = [' '];

    internal static string ConvertRect(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
        var values = value.Split([','], StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => v.Trim());

        if (!string.IsNullOrEmpty(value))
        {
            string[] xywh = value.Split(',');
            if (xywh.Length == 4
                && double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x)
                && double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y)
                && double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
                && double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
            {
                return $"new global::Microsoft.Maui.Graphics.Rect({x}, {y}, {w}, {h})";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.RectConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertColor(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrEmpty(value))
        {
            // Any named colors are ok. Surrounding white spaces are ok.
            if (KnownNamedColors.Contains(value.Trim()))
            {
                return $"global::Microsoft.Maui.Graphics.Color.Parse(\"{value}\")";
            }

            // Check for HEX Color string
            if (RxColorHex.Value.IsMatch(value))
            {
                return $"global::Microsoft.Maui.Graphics.Color.Parse(\"{value}\")";
            }

            var match = RxFuncExpr.Value.Match(value);

            var funcName = match?.Groups?["func"]?.Value;
            var funcValues = match?.Groups?["v"]?.Captures;

            if (!string.IsNullOrEmpty(funcName) && funcValues is not null)
            {
                // ie: argb() needs 4 parameters:
                if (funcValues.Count == funcName?.Length)
                {
                    return $"global::Microsoft.Maui.Graphics.Color.Parse(\"{value}\")";
                }

            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.ColorConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertPoint(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
        if (!string.IsNullOrEmpty(value))
        {
            string[] xy = value.Split(',');
            if (xy.Length == 2 && double.TryParse(xy[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var x)
                && double.TryParse(xy[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var y))
            {
                return $"new global::Microsoft.Maui.Graphics.Point({x}, {y})";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.PointConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertThickness(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update ThicknessTypeDesignConverter.IsValid if making changes here
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();

            if (value.Contains(','))
            { //Xaml
                var thickness = value.Split(',');
                switch (thickness.Length)
                {
                    case 2:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double h)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double v))
                            return $"new global::Microsoft.Maui.Thickness({h}, {v})";
                        break;
                    case 4:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
                            && double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
                            && double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
                            return $"new global::Microsoft.Maui.Thickness({l}, {t}, {r}, {b})";
                        break;
                }
            }
            else if (value.Contains(' '))
            { //CSS
                var thickness = value.Split(' ');
                switch (thickness.Length)
                {
                    case 2:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double v)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
                            return $"new global::Microsoft.Maui.Thickness({h}, {v})";
                        break;
                    case 3:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out h)
                            && double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
                            return $"new global::Microsoft.Maui.Thickness({h}, {t}, {h}, {b})";
                        break;
                    case 4:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out t)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
                            && double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out b)
                            && double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                            return $"new global::Microsoft.Maui.Thickness({l}, {t}, {r}, {b})";
                        break;
                }
            }
            else
            { //single uniform thickness
                if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                    return $"new global::Microsoft.Maui.Thickness({l})";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.ThicknessConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertCornerRadius(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update CornerRadiusDesignTypeConverter.IsValid if making changes here
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();

            if (value.Contains(','))
            { //Xaml
                var cornerRadius = value.Split(',');
                if (cornerRadius.Length == 4
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
                    && double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
                    && double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
                    return $"new global::Microsoft.Maui.CornerRadius({tl}, {tr}, {bl}, {br})";

                if (cornerRadius.Length > 1
                    && cornerRadius.Length < 4
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                    return $"new global::Microsoft.Maui.CornerRadius({l})";
            }
            else if (value.Contains(' '))
            { //CSS
                var cornerRadius = value.Split(' ');
                if (cornerRadius.Length == 2
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
                    return $"new global::Microsoft.Maui.CornerRadius({t}, {b}, {b}, {t})";
                if (cornerRadius.Length == 3
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double trbl)
                    && double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
                    return $"new global::Microsoft.Maui.CornerRadius({tl}, {trbl}, {trbl}, {br})";
                if (cornerRadius.Length == 4
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out tl)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
                    && double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
                    && double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out br))
                    return $"new global::Microsoft.Maui.CornerRadius({tl}, {tr}, {bl}, {br})";

            }
            else
            { //single uniform CornerRadius
                if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                    return $"new global::Microsoft.Maui.CornerRadius({l})";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.CornerRadiusConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertEasing(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        var easingName = value;

        if (!string.IsNullOrWhiteSpace(easingName))
        {
            var parts = easingName.Split('.');
			if (parts.Length == 2 && parts[0].Equals("Easing", StringComparison.OrdinalIgnoreCase))
            {
				easingName = parts[1];
            }

            if (KnownEasingNames.Contains(easingName))
            {
                return $"global::Microsoft.Maui.Easing.{easingName}";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.EasingConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertEnum(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrWhiteSpace(value) && toType is not null && toType.TypeKind == TypeKind.Enum)
        {
            var detectedEnumValue = toType.GetFields().FirstOrDefault(
                f => string.Equals(f.Name, value, StringComparison.OrdinalIgnoreCase));

            if (detectedEnumValue is not null)
            {
                return $"{toType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{detectedEnumValue}";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.EnumTypeConverterConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value, value, toType?.ToDisplayString()));

        return "default";
    }

    internal static string ConvertFlexBasis(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();

            if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return $"global::Microsoft.Maui.Layouts.FlexBasis.Auto";
            }

            if (value.EndsWith("%", StringComparison.OrdinalIgnoreCase)
                && float.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out float relflex))
            {
                return $"new global::Microsoft.Maui.Layouts.FlexBasis({relflex / 100}, true)";
            }

            if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
            {
                return $"new global::Microsoft.Maui.Layouts.FlexBasis({flex}, false)";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.FlexBasisConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertFlowDirection(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            value = value.Trim();

            if (value.Equals("ltr", StringComparison.OrdinalIgnoreCase))
            {
                return "global::Microsoft.Maui.FlowDirection.LeftToRight";
            }

            if (value.Equals("rtl", StringComparison.OrdinalIgnoreCase))
            {
                return "global::Microsoft.Maui.FlowDirection.RightToLeft";
            }

            if (value.Equals("inherit", StringComparison.OrdinalIgnoreCase))
            {
                return "global::Microsoft.Maui.FlowDirection.MatchParent";
            }

            return ConvertEnum(value, toType, reportDiagnostic, xmlLineInfo, filePath);
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.FlowDirectionConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertGridLength(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();

            if(value.Equals("*", StringComparison.OrdinalIgnoreCase))
            {
                return $"global::Microsoft.Maui.GridLength.Star";
            }
            else if (value.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
                {
                    return $"new global::Microsoft.Maui.GridLength({val}, global::Microsoft.Maui.GridUnitType.Star)";
                }
            }
            else if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return $"global::Microsoft.Maui.GridLength.Auto";
            }
            else if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
            {
                return $"new global::Microsoft.Maui.GridLength({val}, global::Microsoft.Maui.GridUnitType.Absolute)";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.GridLengthConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertImageSource(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update ImageSourceDesignTypeConverter.IsValid if making changes here
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();
            
			return Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme != "file" ?
                $"global::Microsoft.Maui.Controls.ImageSource.FromUri(new global::System.Uri(\"{uri}\"))" : $"global::Microsoft.Maui.Controls.ImageSource.FromFile(\"{value}\")";
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.ImageSourceConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }
    
    internal static string ConvertListString(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();
            
            return $"new global::System.Collections.Generic.List<string>(new[] {{ {string.Join(", ", value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => $"\"{v.Trim()}\""))} }})";
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.ListStringConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertPointCollection(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrEmpty(value))
        {
            string[] points = value.Split([' ', ',']);
			var pointCollection = new List<string>();
			double x = 0;
			bool hasX = false;

			foreach (string point in points)
			{
				if (string.IsNullOrWhiteSpace(point)) continue;

				if (double.TryParse(point, NumberStyles.Number, CultureInfo.InvariantCulture, out double number))
				{
					if (!hasX)
					{
						x = number;
						hasX = true;
					}
					else
					{
						pointCollection.Add(ConvertPoint($"{x},{number}", toType, reportDiagnostic, xmlLineInfo, filePath));
						hasX = false;
					}
				}
				else
                {
					reportDiagnostic(Diagnostic.Create(Descriptors.PointCollectionConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

                    return "default";
                }
			}

			if (hasX)
            {
				reportDiagnostic(Diagnostic.Create(Descriptors.PointCollectionConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

                return "default";
            }

			return $"new global::Microsoft.Maui.Controls.PointCollection(new[] {{ {string.Join(", ", pointCollection)} }})";
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.PointCollectionConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertPathGeometry(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();

            // TODO
            return "new global::Microsoft.Maui.Controls.Shapes.PathGeometry()";
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.PathGeometryConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }

    internal static string ConvertStrokeShape(string value, ITypeSymbol toType, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();

            if (value.StartsWith(Ellipse, StringComparison.OrdinalIgnoreCase))
            {
                return "new global::Microsoft.Maui.Controls.Shapes.Ellipse()";
            }

            if (value.StartsWith(Line, StringComparison.OrdinalIgnoreCase))
            {
                var parts = value.Split(Delimiter, 2);
                if (parts.Length != 2)
                {
                    return "new global::Microsoft.Maui.Controls.Shapes.Line()";
                }

                var coordinates = parts[1].Split(',');
                if (coordinates.Length == 2 && double.TryParse(coordinates[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x1)
                    && double.TryParse(coordinates[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y1))
                {
                    return $"new global::Microsoft.Maui.Controls.Shapes.Line {{ X1 = {x1}, Y1 = {y1} }}";
                }
                else if (coordinates.Length == 4 && double.TryParse(coordinates[0], NumberStyles.Number, CultureInfo.InvariantCulture, out x1)
                    && double.TryParse(coordinates[1], NumberStyles.Number, CultureInfo.InvariantCulture, out y1)
                    && double.TryParse(coordinates[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double x2)
                    && double.TryParse(coordinates[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double y2))
                {
                    return $"new global::Microsoft.Maui.Controls.Shapes.Line {{ X1 = {x1}, Y1 = {y1}, X2 = {x2}, Y2 = {y2} }}";
                }
            }

            if (value.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
            {
                var parts = value.Split(Delimiter, 2);
                if (parts.Length != 2)
                {
                    return "new global::Microsoft.Maui.Controls.Shapes.Path()";
                }

                // TODO
                // PathGeometryConverter pathGeometryConverter = new PathGeometryConverter();
                // Geometry pathGeometry = pathGeometryConverter.ConvertFromInvariantString(parts[1]) as Geometry;

                // if (pathGeometry == null)
                // {
                //     return "new global::Microsoft.Maui.Controls.Shapes.Path()";
                // }

                //return new Path { Data = pathGeometry };
            }

            if (value.StartsWith(Polygon, StringComparison.OrdinalIgnoreCase))
            {
                var parts = value.Split(Delimiter, 2);
                if (parts.Length != 2)
                {
                    return "new global::Microsoft.Maui.Controls.Shapes.Polygon()";
                }

                var pointCollection = ConvertPointCollection(parts[1], toType, reportDiagnostic, xmlLineInfo, filePath);

                // If this happens the ConvertPointCollection method already reported an error, but lets still produce valid code.
                if (pointCollection.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    return "new global::Microsoft.Maui.Controls.Shapes.Polyline()";
                }

                return $"new global::Microsoft.Maui.Controls.Shapes.Polygon {{ Points = {pointCollection} }}";
            }

            if (value.StartsWith(Polyline, StringComparison.OrdinalIgnoreCase))
            {
                var parts = value.Split(Delimiter, 2);
                if (parts.Length != 2)
                {
                    return "new global::Microsoft.Maui.Controls.Shapes.Polyline()";
                }

                var pointCollection = ConvertPointCollection(parts[1], toType, reportDiagnostic, xmlLineInfo, filePath);

                // If this happens the ConvertPointCollection method already reported an error, but lets still produce valid code.
                if (pointCollection.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    return "new global::Microsoft.Maui.Controls.Shapes.Polyline()";
                }

                return $"new global::Microsoft.Maui.Controls.Shapes.Polyline {{ Points = {pointCollection} }}";
            }

            if (value.StartsWith(Rectangle, StringComparison.OrdinalIgnoreCase))
            {
                return "new global::Microsoft.Maui.Controls.Shapes.Rectangle()";
            }

            if (value.StartsWith(RoundRectangle, StringComparison.OrdinalIgnoreCase))
            {
                var parts = value.Split(Delimiter, 2);

                var cornerRadius = "new global::Microsoft.Maui.CornerRadius()";

                if (parts.Length > 1)
                {
                    cornerRadius = ConvertCornerRadius(parts[1], toType, reportDiagnostic, xmlLineInfo, filePath);
                }

                return $"new global::Microsoft.Maui.Controls.Shapes.RoundRectangle {{ CornerRadius = {cornerRadius} }}";
            }
        }

        reportDiagnostic(Diagnostic.Create(Descriptors.StrokeShapeConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));

        return "default";
    }
}