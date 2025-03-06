using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		W2DGraphicsView GetNativeBoxView(ShapeViewHandler boxViewHandler) =>
			boxViewHandler.PlatformView;

		Task<bool> GetPlatformIsVisible(ShapeViewHandler boxViewHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeBoxView(boxViewHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}
	}
}