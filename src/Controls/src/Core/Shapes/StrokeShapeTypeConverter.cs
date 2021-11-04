using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public class StrokeShapeTypeConverter : TypeConverter
	{
		public const string Ellipse = "ellipse";
		public const string Line = "line";
		public const string Path = "path";
		public const string Polygon = "polygon";
		public const string Polyline = "polyline";
		public const string Rectangle = "rectangle";
		public const string RoundRectangle = "roundrectangle";

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			   => sourceType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				strValue = strValue.Trim();
				char[] delimiter = { ' ' };

				if (strValue.StartsWith(Ellipse, true, culture))
				{
					return new Ellipse();
				}

				if (strValue.StartsWith(Line, true, culture))
				{
					var parts = strValue.Split(delimiter, 2);

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
					var parts = strValue.Split(delimiter, 2);

					PathGeometryConverter pathGeometryConverter = new PathGeometryConverter();
					Geometry pathGeometry = pathGeometryConverter.ConvertFromInvariantString(parts[1]) as Geometry;

					if (pathGeometry == null)
						return new Path();

					return new Path { Data = pathGeometry };
				}

				if (strValue.StartsWith(Polygon, true, culture))
				{
					var parts = strValue.Split(delimiter, 2);

					PointCollectionConverter pointCollectionConverter = new PointCollectionConverter();
					PointCollection points = pointCollectionConverter.ConvertFromString(parts[1]) as PointCollection;

					if (points == null || points.Count == 0)
						return new Polygon();

					return new Polygon { Points = points };
				}

				if (strValue.StartsWith(Polyline, true, culture))
				{
					var parts = strValue.Split(delimiter, 2);

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
					var parts = strValue.Split(delimiter, 2);

					CornerRadiusTypeConverter cornerRadiusTypeConverter = new CornerRadiusTypeConverter();
					CornerRadius cornerRadius = (CornerRadius)cornerRadiusTypeConverter.ConvertFromString(parts[1]);

					return new RoundRectangle { CornerRadius = cornerRadius };
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Shape)}");
		}
	}
}