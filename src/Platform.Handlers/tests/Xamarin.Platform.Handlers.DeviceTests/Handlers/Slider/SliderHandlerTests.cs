using System.Drawing;
using System.Threading.Tasks;
using Xamarin.Platform.Handlers.DeviceTests.Stubs;
using Xunit;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class SliderHandlerTests : HandlerTestBase<SliderHandler>
	{
#if __ANDROID__
		[Fact(Skip = "Currently Fails on Android")]
#else
		[Fact()]
#endif

		public async Task ValueInitializesCorrectly()
		{
			var slider = new SliderStub()
			{
				Maximum = 1,
				Minimum = 0,
				Value = 0.5
			};

			await ValidatePropertyInitValue(slider, () => slider.Value, GetNativeProgress, slider.Value);
		}

		[Fact]
		public async Task MaximumInitializesCorrectly()
		{
			var slider = new SliderStub()
			{
				Maximum = 1
			};

			await ValidatePropertyInitValue(slider, () => slider.Maximum, GetNativeMaximum, slider.Maximum);
		}

		[Fact]
		public async Task ThumbColorInitializesCorrectly()
		{
			var slider = new SliderStub()
			{
				ThumbColor = Color.Purple
			};

			await ValidateNativeThumbColor(slider, Color.Purple);
		}
	}
}
