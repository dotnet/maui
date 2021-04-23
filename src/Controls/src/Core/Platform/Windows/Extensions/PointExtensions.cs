using Microsoft.Maui.Graphics;
using WPoint = Windows.Foundation.Point;

namespace Microsoft.Maui.Controls.Platform
{
	public static class PointExtensions
	{
		public static WPoint ToWindows(this Point point)
		{
			return new WPoint(point.X, point.Y);
		}
	}
}