#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents a region composed of one or more rectangles.</summary>
	public struct Region : IEquatable<Region>
	{
		// While Regions are currently rectangular, they could in the future be transformed into any shape.
		// As such the internals of how it keeps shapes is hidden, so that future internal changes can occur to support shapes
		// such as circles if required, without affecting anything else.

		IReadOnlyList<Rect> Regions { get; }
		readonly Thickness? _inflation;

		Region(IList<Rect> positions) : this()
		{
			Regions = new ReadOnlyCollection<Rect>(positions);
		}

		Region(IList<Rect> positions, Thickness inflation) : this(positions)
		{
			_inflation = inflation;
		}

		public static Region FromRectangles(IEnumerable<Rect> rectangles)
		{
			var list = rectangles.ToList();
			return new Region(list);
		}

		/// <summary>Creates a region from line heights and positions.</summary>
		public static Region FromLines(double[] lineHeights, double maxWidth, double startX, double endX, double startY)
		{
			var positions = new List<Rect>();
			var endLine = lineHeights.Length - 1;
			var lineHeightTotal = startY;

			for (var i = 0; i <= endLine; i++)
				if (endLine != 0) // MultiLine
				{
					if (i == 0) // First Line
						positions.Add(new Rect(startX, lineHeightTotal, maxWidth - startX, lineHeights[i]));

					else if (i != endLine) // Middle Line
						positions.Add(new Rect(0, lineHeightTotal, maxWidth, lineHeights[i]));

					else // End Line
						positions.Add(new Rect(0, lineHeightTotal, endX, lineHeights[i]));

					lineHeightTotal += lineHeights[i];
				}
				else // SingleLine
					positions.Add(new Rect(startX, lineHeightTotal, endX - startX, lineHeights[i]));

			return new Region(positions);
		}

		/// <summary>Determines whether the region contains the specified point.</summary>
		public bool Contains(Point pt)
		{
			return Contains(pt.X, pt.Y);
		}

		/// <summary>Determines whether the region contains the specified coordinates.</summary>
		public bool Contains(double x, double y)
		{
			if (Regions == null)
				return false;

			for (int i = 0; i < Regions.Count; i++)
				if (Regions[i].Contains(x, y))
					return true;

			return false;
		}

		/// <summary>Deflates the region by reversing any previous inflation.</summary>
		public Region Deflate()
		{
			if (_inflation == null)
				return this;

			return Inflate(_inflation.Value.Left * -1, _inflation.Value.Top * -1, _inflation.Value.Right * -1, _inflation.Value.Bottom * -1);
		}

		/// <summary>Inflates the region uniformly by the specified size.</summary>
		public Region Inflate(double size)
		{
			return Inflate(size, size, size, size);
		}

		/// <summary>Inflates the region by the specified amounts on each side.</summary>
		public Region Inflate(double left, double top, double right, double bottom)
		{
			if (Regions == null)
				return this;

			Rect[] rectangles = new Rect[Regions.Count];
			for (int i = 0; i < Regions.Count; i++)
			{
				var region = Regions[i];

				region.Top -= top;

				region.Left -= left;
				region.Width += right + left;

				region.Height += bottom + top;

				rectangles[i] = region;
			}

			var inflation = new Thickness(_inflation == null ? left : left + _inflation.Value.Left,
									   _inflation == null ? top : top + _inflation.Value.Top,
									   _inflation == null ? right : right + _inflation.Value.Right,
									   _inflation == null ? bottom : bottom + _inflation.Value.Bottom);

			return new Region(rectangles, inflation);
		}

		public bool Equals(Region other) =>
			Regions == other.Regions && _inflation == other._inflation;

		public override bool Equals(object obj) => obj is Region other && Equals(other);

		public override int GetHashCode() => Regions.GetHashCode() ^ _inflation?.GetHashCode() ?? 0;

		public static bool operator ==(Region left, Region right) => left.Equals(right);

		public static bool operator !=(Region left, Region right) => !(left == right);
	}
}