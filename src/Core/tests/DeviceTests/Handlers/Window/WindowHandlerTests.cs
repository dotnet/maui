using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class WindowHandlerTests : HandlerTestBase
	{
		[Theory]
		[InlineData(-1)]
		[InlineData(float.NaN)]
		public async Task WindowHandlerDoesNotUseValueFromIWindow(float testDensity)
		{
			var control = new WindowStub();

			// set an invalid value so we know the test doesn't accidentally use it
			control.SetDisplayDensity(testDensity);

			var density = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<WindowHandler>(control);

				return handler.InvokeWithResult(nameof(IWindow.RequestDisplayDensity), new DisplayDensityRequest());
			});

			Assert.NotEqual(testDensity, density);
		}

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