using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class WindowHandlerTests : HandlerTestBase
	{
		// TODO: think of how to set up windows for the headless runner which is not
		//       a real maui app because we are trying to test a maui app
		// [Fact]
		// public async Task WindowHasReasonableDisplayDensity()
		// {
		// 	var window = MauiProgram.DefaultTestApp.Windows[0];
		// 	var handler = window.Handler!;

		// 	var req = new DisplayDensityRequest();

		// 	var density = await InvokeOnMainThreadAsync(() => handler.InvokeWithResult(nameof(IWindow.RequestDisplayDensity), req));

		// 	Assert.Equal(density, req.Result);
		// 	Assert.InRange(density, 0.1f, 4f);
		// }
	}
}