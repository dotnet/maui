using System.Maui.Graphics;
using System.Maui.Core;

namespace System.Maui.Shapes
{
	public class RoundedRectangle : IShape
	{
		public RoundedRectangle(float cornerRadius)
		{
			CornerRadius = cornerRadius;
		}

		public RoundedRectangle(float cornerRadiusTopLeft, float cornerRadiusTopRight, float cornerRadiusBottomRight, float cornerRadiusBottomLeft)
		{
		
		}

		public float CornerRadius { get; set; }

		public Path PathForBounds(System.Maui.Rectangle rect)
		{
			var path = new Path();
			path.AppendRoundedRectangle(rect, CornerRadius);
			return path;
		}
	}
}
