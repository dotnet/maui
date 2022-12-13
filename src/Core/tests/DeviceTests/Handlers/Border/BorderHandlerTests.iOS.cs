using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderHandlerTests
	{
		ContentView GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;
	}
}
