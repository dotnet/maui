using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		PlatformGraphicsView GetNativeBoxView(ShapeViewHandler boxViewHandler) =>
			boxViewHandler.PlatformView;
	}
}