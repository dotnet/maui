using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests
{
	public partial class GraphicsViewHandlerTests
	{
		PlatformTouchGraphicsView GetPlatformGraphicsView(GraphicsViewHandler graphicsViewHandler) =>
			graphicsViewHandler.PlatformView;

		Task ValidateHasColor(IGraphicsView graphicsView, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var PlatformGraphicsView = GetPlatformGraphicsView(CreateHandler(graphicsView));
				action?.Invoke();
				PlatformGraphicsView.AssertContainsColorAsync(color);
			});
		}
	}
}