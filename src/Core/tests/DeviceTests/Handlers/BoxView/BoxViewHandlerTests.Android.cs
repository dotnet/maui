using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewHandlerTests
	{
		MauiShapeView GetNativeBoxView(ShapeViewHandler boxViewViewHandler) =>
			boxViewViewHandler.PlatformView;

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