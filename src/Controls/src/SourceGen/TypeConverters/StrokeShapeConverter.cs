using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class StrokeShapeConverter : ISGTypeConverter
{
	private const string Ellipse = nameof(Ellipse);
	private const string Line = nameof(Line);
	private const string Path = nameof(Path);
	private const string Polygon = nameof(Polygon);
	private const string Polyline = nameof(Polyline);
	private const string Rectangle = nameof(Rectangle);
	private const string RoundRectangle = nameof(RoundRectangle);
	private static readonly char[] Delimiter = [' '];

	public IEnumerable<string> SupportedTypes => new[] { "Shape", "Microsoft.Maui.Controls.Shapes.Shape" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
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

				var coordinates = parts[1].Split([',']);
				if (coordinates.Length == 2 && double.TryParse(coordinates[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x1)
					&& double.TryParse(coordinates[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y1))
				{
					return $"new global::Microsoft.Maui.Controls.Shapes.Line {{ X1 = {FormatInvariant(x1)}, Y1 = {FormatInvariant(y1)} }}";
				}
				else if (coordinates.Length == 4 && double.TryParse(coordinates[0], NumberStyles.Number, CultureInfo.InvariantCulture, out x1)
					&& double.TryParse(coordinates[1], NumberStyles.Number, CultureInfo.InvariantCulture, out y1)
					&& double.TryParse(coordinates[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double x2)
					&& double.TryParse(coordinates[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double y2))
				{
					return $"new global::Microsoft.Maui.Controls.Shapes.Line {{ X1 = {FormatInvariant(x1)}, Y1 = {FormatInvariant(y1)}, X2 = {FormatInvariant(x2)}, Y2 = {FormatInvariant(y2)} }}";
				}
			}

			if (value.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Split(Delimiter, 2);
				if (parts.Length != 2)
				{
					return "new global::Microsoft.Maui.Controls.Shapes.Path()";
				}

				// TODO: Implement proper path geometry conversion
				return "new global::Microsoft.Maui.Controls.Shapes.Path()";
			}

			if (value.StartsWith(Polygon, StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Split(Delimiter, 2);
				if (parts.Length != 2)
				{
					return "new global::Microsoft.Maui.Controls.Shapes.Polygon()";
				}

				var pointCollectionConverter = new PointCollectionConverter();
				var pointCollection = pointCollectionConverter.Convert(parts[1], node, toType, context);

				// If this happens the ConvertPointCollection method already reported an error, but lets still produce valid code.
				if (pointCollection.Equals("default", StringComparison.OrdinalIgnoreCase))
				{
					return "new global::Microsoft.Maui.Controls.Shapes.Polygon()";
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

				var pointCollectionConverter = new PointCollectionConverter();
				var pointCollection = pointCollectionConverter.Convert(parts[1], node, toType, context);

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
					var cornerRadiusConverter = new CornerRadiusConverter();
					cornerRadius = cornerRadiusConverter.Convert(parts[1], node, toType, context);
				}

				return $"new global::Microsoft.Maui.Controls.Shapes.RoundRectangle {{ CornerRadius = {cornerRadius} }}";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.StrokeShapeConversionFailed);
		return "default";
	}
}