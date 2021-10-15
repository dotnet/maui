using System.Globalization;
using System.IO;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	public static class PathExtensions
	{
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

		public static PathF AsScaledPath(
			this PathF target,
			float scale)
		{
			var scaledPath = new PathF(target);
			var transform = Matrix3x2.CreateScale(scale);
			scaledPath.Transform(transform);
			return scaledPath;
		}
	}
}
