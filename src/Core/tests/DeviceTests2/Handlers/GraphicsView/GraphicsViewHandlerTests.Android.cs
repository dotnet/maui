using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class GraphicsViewHandlerTests
	{
		NativeGraphicsView GetNativeGraphicsView(GraphicsViewHandler graphicsViewHandler) =>
			graphicsViewHandler.NativeView;

		Task ValidateHasColor(IGraphicsView graphicsView, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeGraphicsView = GetNativeGraphicsView(CreateHandler(graphicsView));
				action?.Invoke();
				nativeGraphicsView.AssertContainsColor(color);
			});
		}
	}
}