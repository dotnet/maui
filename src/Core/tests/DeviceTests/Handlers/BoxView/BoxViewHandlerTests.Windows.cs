using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewHandlerTests
	{
		W2DGraphicsView GetNativeBoxView(ShapeViewHandler boxViewHandler) =>
			boxViewHandler.PlatformView;

		Task ValidateHasColor(IShapeView boxView, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeBoxView = GetNativeBoxView(CreateHandler(boxView));
				action?.Invoke();
				nativeBoxView.AssertContainsColorAsync(color);
			});
		}
	}
}