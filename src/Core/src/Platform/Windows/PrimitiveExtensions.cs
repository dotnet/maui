using WPoint = Windows.Foundation.Point;

namespace Microsoft.Maui
{
	public static class PrimitiveExtensions
	{
		public static WPoint ToNative(this Point point)
		{
			return new WPoint(point.X, point.Y);
		}
	}
}