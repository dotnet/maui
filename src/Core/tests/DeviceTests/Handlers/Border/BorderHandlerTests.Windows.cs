using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderHandlerTests
	{
		ContentPanel GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;
	}
}