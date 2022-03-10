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
			var control = new WindowStub();

			var density = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<WindowHandler>(control);

				return handler.InvokeWithResult(nameof(IWindow.RequestDisplayDensity), new DisplayDensityRequest());
			});

			Assert.InRange(density, 0.1f, 4f);
		}
	}
}