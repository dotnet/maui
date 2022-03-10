using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class WindowHandlerTests : HandlerTestBase
	{
		[Fact]
		public async Task WindowHasReasonableDisplayDensity()
		{
			var window = MauiProgram.DefaultTestApp.Windows[0];
			var handler = window.Handler!;

			var req = new DisplayDensityRequest();

			var density = await InvokeOnMainThreadAsync(() => handler.InvokeWithResult(nameof(IWindow.RequestDisplayDensity), req));

			Assert.Equal(density, req.Result);
			Assert.InRange(density, 0.1f, 4f);
		}
	}
}