#if WINDOWS_UWP
using WPoint = Windows.Foundation.Point;

namespace Xamarin.Forms.Platform.UWP
#else
using WPoint = System.Windows.Point;

namespace Xamarin.Forms.Platform.WPF
#endif
{
	public static class PointExtensions
	{
		public static WPoint ToWindows(this Point point)
		{
			return new WPoint(point.X, point.Y);
		}
	}
}