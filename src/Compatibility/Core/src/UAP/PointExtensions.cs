#if WINDOWS_UWP
using WPoint = Windows.Foundation.Point;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WPoint = System.Windows.Point;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
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