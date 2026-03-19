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
	}
}