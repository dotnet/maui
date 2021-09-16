using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewHandlerTests
	{
		MauiBoxView GetNativeBoxView(BoxViewHandler boxViewViewHandler) =>
			boxViewViewHandler.NativeView;

		Task ValidateHasColor(IBoxView boxView, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeBoxView = GetNativeBoxView(CreateHandler(boxView));
				action?.Invoke();
				nativeBoxView.AssertContainsColor(color);
			});
		}
	}
}