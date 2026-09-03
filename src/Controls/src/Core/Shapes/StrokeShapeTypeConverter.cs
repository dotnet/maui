#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.StrokeShapeTypeConverter")]
	public class StrokeShapeTypeConverter : TypeConverter
	{
		public const string Ellipse = nameof(Ellipse);
		public const string Line = nameof(Line);
		public const string Path = nameof(Path);
		public const string Polygon = nameof(Polygon);
		public const string Polyline = nameof(Polyline);
		public const string Rectangle = nameof(Rectangle);
		public const string RoundRectangle = nameof(RoundRectangle);

		internal static readonly char[] Delimiter = { ' ' };

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			   => sourceType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				strValue = strValue.Trim();

				if (strValue.StartsWith(Ellipse, true, culture))
				{
					return new Ellipse();
				}

				if (strValue.StartsWith(Line, true, culture))
				{
					var parts = strValue.Split(Delimiter, 2);
					if (parts.Length != 2)
						return new Line();

					PointCollectionConverter pointCollectionConverter = new PointCollectionConverter();
					PointCollection points = pointCollectionConverter.ConvertFromString(parts[1]) as PointCollection;

					if (points == null || points.Count == 0)
						return new Line();

					Point p1 = points[0];

					if (points.Count == 1)
						return new Line { X1 = p1.X, Y1 = p1.Y };

					Point p2 = points[1];

					return new Line { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y };
				}

				if (strValue.StartsWith(Path, true, culture))
				{
					var parts = strValue.Split(Delimiter, 2);
					if (parts.Length != 2)
						return new Path();

					PathGeometryConverter pathGeometryConverter = new PathGeometryConverter();
					Geometry pathGeometry = pathGeometryConverter.ConvertFromInvariantString(parts[1]) as Geometry;

					if (pathGeometry == null)
						return new Path();

					return new Path { Data = pathGeometry };
				}

				if (strValue.StartsWith(Polygon, true, culture))
				{
					var parts = strValue.Split(Delimiter, 2);
					if (parts.Length != 2)
						return new Polygon();

					PointCollectionConverter pointCollectionConverter = new PointCollectionConverter();
					PointCollection points = pointCollectionConverter.ConvertFromString(parts[1]) as PointCollection;

					if (points == null || points.Count == 0)
						return new Polygon();

					return new Polygon { Points = points };
				}

				if (strValue.StartsWith(Polyline, true, culture))
				{
					var parts = strValue.Split(Delimiter, 2);
					if (parts.Length != 2)
						return new Polyline();

					PointCollectionConverter pointCollectionConverter = new PointCollectionConverter();
					PointCollection points = pointCollectionConverter.ConvertFromString(parts[1]) as PointCollection;

					if (points == null || points.Count == 0)
						return new Polyline();

					return new Polyline { Points = points };
				}

				if (strValue.StartsWith(Rectangle, true, culture))
				{
					return new Rectangle();
				}

				if (strValue.StartsWith(RoundRectangle, true, culture))
				{
					var parts = strValue.Split(Delimiter, 2);

					CornerRadius cornerRadius = new CornerRadius();

					if (parts.Length > 1)
					{
						CornerRadiusTypeConverter cornerRadiusTypeConverter = new CornerRadiusTypeConverter();
						cornerRadius = (CornerRadius)cornerRadiusTypeConverter.ConvertFromString(parts[1]);
					}

					return new RoundRectangle { CornerRadius = cornerRadius };
				}

				// Support for providing a double. This handles Border CSS support.
				if (double.TryParse(strValue, out double radius))
				{
					return new RoundRectangle { CornerRadius = new CornerRadius(radius) };
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Shape)}");
		}
	}
}