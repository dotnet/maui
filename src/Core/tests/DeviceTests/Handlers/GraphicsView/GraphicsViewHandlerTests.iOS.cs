using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class GraphicsViewHandlerTests
	{
		PlatformGraphicsView GetPlatformGraphicsView(GraphicsViewHandler graphicsViewHandler) =>
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