using System.Globalization;
using System.IO;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for working with <see cref="PathF"/> objects.
	/// </summary>
	public static class PathExtensions
	{
		/// <summary>
		/// Converts a path to an SVG-style path definition string.
		/// </summary>
		/// <param name="path">The path to convert.</param>
		/// <param name="ppu">Points per unit scaling factor (default is 1).</param>
		/// <returns>A string representation of the path using SVG path commands.</returns>
		public static string ToDefinitionString(this PathF path, float ppu = 1)
		{
			var writer = new StringWriter();

			for (var i = 0; i < path.OperationCount; i++)
			{
				var type = path.GetSegmentType(i);
				var points = path.GetPointsForSegment(i);

				if (type == PathOperation.Move)
				{
					writer.Write("M");
					WritePoint(writer, points[0], ppu);
				}
				else if (type == PathOperation.Line)
				{
					writer.Write(" L");
					WritePoint(writer, points[0], ppu);
				}
				else if (type == PathOperation.Quad)
				{
					writer.Write(" Q");
					WritePoint(writer, points[0], ppu);
					writer.Write(" ");
					WritePoint(writer, points[1], ppu);
				}
				else if (type == PathOperation.Cubic)
				{
					writer.Write(" C");
					WritePoint(writer, points[0], ppu);
					writer.Write(" ");
					WritePoint(writer, points[1], ppu);
					writer.Write(" ");
					WritePoint(writer, points[2], ppu);
				}
				else if (type == PathOperation.Close)
				{
					writer.Write(" Z ");
				}
			}

			return writer.ToString();
		}

		private static void WritePoint(StringWriter writer, PointF point, float ppu)
		{
			float x = point.X * ppu;
			float y = point.Y * ppu;

			string cx = x.ToString(CultureInfo.InvariantCulture);
			string cy = y.ToString(CultureInfo.InvariantCulture);

			writer.Write(cx);
			writer.Write(" ");
			writer.Write(cy);
		}

		/// <summary>
		/// Creates a new path by scaling the target path uniformly.
		/// </summary>
		/// <param name="target">The path to scale.</param>
		/// <param name="scale">The uniform scale factor to apply to both x and y dimensions.</param>
		/// <returns>A new scaled path.</returns>
		public static PathF AsScaledPath(
			this PathF target,
			float scale)
		{
			var scaledPath = new PathF(target);
			var transform = Matrix3x2.CreateScale(scale);
			scaledPath.Transform(transform);
			return scaledPath;
		}

		/// <summary>
		/// Creates a new path by scaling the target path with separate x and y scale factors.
		/// </summary>
		/// <param name="target">The path to scale.</param>
		/// <param name="xScale">The scale factor to apply to the x dimension.</param>
		/// <param name="yScale">The scale factor to apply to the y dimension.</param>
		/// <returns>A new scaled path.</returns>
		public static PathF AsScaledPath(
			this PathF target,
			float xScale,
			float yScale)
		{
			var scaledPath = new PathF(target);
			var transform = Matrix3x2.CreateScale(xScale, yScale);
			scaledPath.Transform(transform);
			return scaledPath;
		}
	}
}
