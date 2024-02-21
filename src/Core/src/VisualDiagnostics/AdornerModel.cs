using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	internal class AdornerModel
	{
		Rect boundingBox;
		List<Rect> marginZones = new List<Rect>();

		public Rect BoundingBox => boundingBox;
		public IReadOnlyList<Rect> MarginZones => marginZones;


		public void Update(Rect rect, Thickness margin, Matrix4x4 transformToRoot, double density)
		{
			double unitsPerPixel = 1 / density;
			boundingBox = DpiHelper.RoundToPixel(rect, unitsPerPixel);

			marginZones.Clear();

			if (margin == new Thickness())
			{
				return;
			}

			// If transform to root is more than just an offset (i.e. element or
			// some of its ancestors are scaled/rotated/etc.) then no margins to
			// render. This matches WPF, UWP and WinUI adorners.
			if (!Tolerances.AreClose(transformToRoot.M11, 1) ||
				!Tolerances.AreClose(transformToRoot.M22, 1) ||
				!Tolerances.AreClose(transformToRoot.M12, 0) ||
				!Tolerances.AreClose(transformToRoot.M21, 0))
			{
				return;
			}

			// Create up to 4 rectangles for margins. Keep in mind that some of
			// margin values can be negative, e.g. Margin="-10, 20, 30, -40".

			// Left
			if (!Tolerances.NearZero(margin.Left))
			{
				Rect rc = new Rect();
				rc.Left = Math.Min(rect.Left - margin.Left, rect.Left);
				rc.Width = Math.Abs(margin.Left);
				rc.Top = rect.Top;
				rc.Bottom = rect.Bottom;
				TryAddMarginZone(rc, unitsPerPixel);
			}

			// Right
			if (!Tolerances.NearZero(margin.Right))
			{
				Rect rc = new Rect();
				rc.Left = Math.Min(rect.Right, rect.Right + margin.Right);
				rc.Width = Math.Abs(margin.Right);
				rc.Top = rect.Top;
				rc.Bottom = rect.Bottom;
				TryAddMarginZone(rc, unitsPerPixel);
			}

			// Top
			if (!Tolerances.NearZero(margin.Top))
			{
				Rect rc = new Rect();
				rc.Left = rect.Left - Math.Max(0, margin.Left);
				rc.Right = rect.Right + Math.Max(0, margin.Right);
				rc.Top = Math.Min(rect.Top - margin.Top, rect.Top);
				rc.Height = Math.Abs(margin.Top);
				TryAddMarginZone(rc, unitsPerPixel);
			}

			// Bottom
			if (!Tolerances.NearZero(margin.Bottom))
			{
				Rect rc = new Rect();
				rc.Left = rect.Left - Math.Max(0, margin.Left);
				rc.Right = rect.Right + Math.Max(0, margin.Right);
				rc.Top = Math.Min(rect.Bottom + margin.Bottom, rect.Bottom);
				rc.Height = Math.Abs(margin.Bottom);
				TryAddMarginZone(rc, unitsPerPixel);
			}
		}

		void TryAddMarginZone(Rect rect, double unitsPerPixel)
		{
			Rect rc = DpiHelper.RoundToPixel(rect, unitsPerPixel);
			if (!rc.IsEmpty)
				marginZones.Add(rc);
		}

		/// <summary>
		/// DPI related utilities.
		/// </summary>
		private static class DpiHelper
		{
			/// <summary>
			/// Rounds unit value to nearest pixel.
			/// </summary>
			/// <returns>
			/// Rounded value in units.
			/// </returns>
			public static double RoundToPixel(double units, double unitsPerPixel)
			{
				double pixels = units / unitsPerPixel;
				double floorPixels = (double)Math.Floor(pixels);
				pixels = Tolerances.LessThan(pixels, floorPixels + 0.5) ?
					floorPixels : (double)Math.Ceiling(pixels);
				return pixels * unitsPerPixel;
			}

			/// <summary>
			/// Rounds point X and Y coordinates to nearest pixel.
			/// </summary>
			public static Point RoundToPixel(Point point, double unitsPerPixel)
			{
				Point pixelPoint = new Point(
					DpiHelper.RoundToPixel(point.X, unitsPerPixel),
					DpiHelper.RoundToPixel(point.Y, unitsPerPixel));
				return pixelPoint;
			}

			/// <summary>
			/// Rounds rectangle corner coordinates to nearest pixel.
			/// </summary>
			public static Rect RoundToPixel(Rect rect, double unitsPerPixel)
			{
				double left = DpiHelper.RoundToPixel(rect.Left, unitsPerPixel);
				double top = DpiHelper.RoundToPixel(rect.Top, unitsPerPixel);
				double right = DpiHelper.RoundToPixel(rect.Right, unitsPerPixel);
				double bottom = DpiHelper.RoundToPixel(rect.Bottom, unitsPerPixel);
				return new Rect(left, top, right - left, bottom - top);
			}
		}

		/// <summary>
		/// Helper utilities to compare double values.
		/// </summary>
		private static class Tolerances
		{
			const double Epsilon = 2.2204460492503131e-016;
			const double ZeroThreshold = 2.2204460492503131e-015;

			public static bool AreClose(Point point1, Point point2)
			{
				if (Tolerances.AreClose(point1.X, point2.X) && Tolerances.AreClose(point1.Y, point2.Y))
				{
					return true;
				}

				return false;
			}

			public static bool NearZero(double value)
			{
				return Math.Abs(value) < Tolerances.ZeroThreshold;
			}

			public static bool AreClose(double value1, double value2)
			{
				//in case they are Infinities (then epsilon check does not work)
				if (value1 == value2)
					return true;
				// This computes (|value1-value2| / (|value1| + |value2| + 10.0)) < Tolerances.Epsilon
				double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * Tolerances.Epsilon;
				double delta = value1 - value2;
				return (-eps < delta) && (eps > delta);
			}

			public static bool GreaterThan(double value1, double value2)
			{
				if (value1 > value2)
				{
					return !Tolerances.AreClose(value1, value2);
				}
				return false;
			}

			public static bool GreaterThanOrClose(double value1, double value2)
			{
				if (value1 <= value2)
				{
					return Tolerances.AreClose(value1, value2);
				}
				return true;
			}

			public static bool LessThan(double value1, double value2)
			{
				if (value1 < value2)
				{
					return !Tolerances.AreClose(value1, value2);
				}
				return false;
			}

			public static bool LessThanOrClose(double value1, double value2)
			{
				if (value1 >= value2)
				{
					return Tolerances.AreClose(value1, value2);
				}
				return true;
			}
		}
	}
}

