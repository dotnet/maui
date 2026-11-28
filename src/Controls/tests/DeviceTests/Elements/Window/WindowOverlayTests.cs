using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory;

[Category(TestCategory.WindowOverlay)]
public class WindowOverlayTests : ControlsHandlerTestBase
{
	[Fact("Does Not Leak")]
	public async Task DoesNotLeak()
	{
		WeakReference viewReference = null;

		{
			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, _ =>
			{
				var overlay = new WindowOverlay(window);
				viewReference = new(overlay);
				window.AddOverlay(overlay);
#if WINDOWS
				var graphicsView = Assert.IsType<Microsoft.Maui.Graphics.Win2D.W2DGraphicsView>(overlay.GraphicsView);
				Assert.Same(overlay, graphicsView.Drawable);
#endif
				window.RemoveOverlay(overlay);
#if WINDOWS
				Assert.Null(graphicsView.Drawable);
#endif
			});
		}

		await AssertionExtensions.WaitForGC(viewReference);
	}
}
