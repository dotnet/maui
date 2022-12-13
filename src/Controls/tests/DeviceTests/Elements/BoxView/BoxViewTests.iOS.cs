using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		MauiShapeView GetNativeBoxView(ShapeViewHandler boxViewHandler) =>
			boxViewHandler.PlatformView;
	}
}