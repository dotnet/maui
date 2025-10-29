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
				var ellipseType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Ellipse")!;
				return $"new {ellipseType.ToFQDisplayString()}()";
			}

			if (value.StartsWith(Line, StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Split(Delimiter, 2);
				if (parts.Length != 2)
				{
					var lineType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Line")!;
					return $"new {lineType.ToFQDisplayString()}()";
				}

				var coordinates = parts[1].Split([',']);
				if (coordinates.Length == 2 && double.TryParse(coordinates[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x1)
					&& double.TryParse(coordinates[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y1))
				{
					var lineType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Line")!;
					return $"new {lineType.ToFQDisplayString()} {{ X1 = {FormatInvariant(x1)}, Y1 = {FormatInvariant(y1)} }}";
				}
				else if (coordinates.Length == 4 && double.TryParse(coordinates[0], NumberStyles.Number, CultureInfo.InvariantCulture, out x1)
					&& double.TryParse(coordinates[1], NumberStyles.Number, CultureInfo.InvariantCulture, out y1)
					&& double.TryParse(coordinates[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double x2)
					&& double.TryParse(coordinates[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double y2))
				{
					var lineType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Line")!;
					return $"new {lineType.ToFQDisplayString()} {{ X1 = {FormatInvariant(x1)}, Y1 = {FormatInvariant(y1)}, X2 = {FormatInvariant(x2)}, Y2 = {FormatInvariant(y2)} }}";
				}
			}

			if (value.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Split(Delimiter, 2);
				if (parts.Length != 2)
				{
					var pathType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Path")!;
					return $"new {pathType.ToFQDisplayString()}()";
				}

				// TODO: Implement proper path geometry conversion
				var pathFallbackType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Path")!;
				return $"new {pathFallbackType.ToFQDisplayString()}()";
			}

			if (value.StartsWith(Polygon, StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Split(Delimiter, 2);
				if (parts.Length != 2)
				{
					var polygonType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Polygon")!;
					return $"new {polygonType.ToFQDisplayString()}()";
				}

				var pointCollectionConverter = new PointCollectionConverter();
				var pointCollection = pointCollectionConverter.Convert(parts[1], node, toType, context);

				// If this happens the ConvertPointCollection method already reported an error, but lets still produce valid code.
				if (pointCollection.Equals("default", StringComparison.OrdinalIgnoreCase))
				{
					var polygonType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Polygon")!;
					return $"new {polygonType.ToFQDisplayString()}()";
				}

				var polygonPointsType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Polygon")!;
				return $"new {polygonPointsType.ToFQDisplayString()} {{ Points = {pointCollection} }}";
			}

			if (value.StartsWith(Polyline, StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Split(Delimiter, 2);
				if (parts.Length != 2)
				{
					var polylineType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Polyline")!;
					return $"new {polylineType.ToFQDisplayString()}()";
				}

				var pointCollectionConverter = new PointCollectionConverter();
				var pointCollection = pointCollectionConverter.Convert(parts[1], node, toType, context);

				// If this happens the ConvertPointCollection method already reported an error, but lets still produce valid code.
				if (pointCollection.Equals("default", StringComparison.OrdinalIgnoreCase))
				{
					var polylineFallbackType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Polyline")!;
					return $"new {polylineFallbackType.ToFQDisplayString()}()";
				}

				var polylinePointsType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Polyline")!;
				return $"new {polylinePointsType.ToFQDisplayString()} {{ Points = {pointCollection} }}";
			}

			if (value.StartsWith(Rectangle, StringComparison.OrdinalIgnoreCase))
			{
				var rectangleType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Rectangle")!;
				return $"new {rectangleType.ToFQDisplayString()}()";
			}

			if (value.StartsWith(RoundRectangle, StringComparison.OrdinalIgnoreCase))
			{
				var parts = value.Split(Delimiter, 2);

				var cornerRadiusType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.CornerRadius")!;
				var cornerRadius = $"new {cornerRadiusType.ToFQDisplayString()}()";

				if (parts.Length > 1)
				{
					var cornerRadiusConverter = new CornerRadiusConverter();
					cornerRadius = cornerRadiusConverter.Convert(parts[1], node, toType, context);
				}

				var roundRectangleType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.RoundRectangle")!;
				return $"new {roundRectangleType.ToFQDisplayString()} {{ CornerRadius = {cornerRadius} }}";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.StrokeShapeConversionFailed);
		return "default";
	}
}