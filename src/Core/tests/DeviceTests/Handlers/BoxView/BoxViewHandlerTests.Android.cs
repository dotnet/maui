using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
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
				nativeBoxView.AssertContainsColor(color);
			});
		}

		[Fact(DisplayName = "Control meets basic accessibility requirements")]
		[Category(TestCategory.Accessibility)]
		public async Task PlatformViewIsAccessible()
		{
			var view = new BoxViewStub();
			await AssertPlatformViewIsAccessible(view);
		}
	}
}