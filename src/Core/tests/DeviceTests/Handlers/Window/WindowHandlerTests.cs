using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class WindowHandlerTests : HandlerTestBase
	{
		[Fact]
		public async Task WindowHasReasonableDisplayDensity()
		{
			var handler = new WindowHandlerProxyStub(
				commandMapper: new()
				{
					[nameof(IWindow.RequestDisplayDensity)] = WindowHandler.MapRequestDisplayDensity
				});

			InitializeViewHandler(new WindowStub(), handler);

			var req = new DisplayDensityRequest();

			var density = await InvokeOnMainThreadAsync(() => handler.InvokeWithResult(nameof(IWindow.RequestDisplayDensity), req));

			Assert.Equal(density, req.Result);
			Assert.InRange(density, 0.1f, 4f);
		}
	}
}