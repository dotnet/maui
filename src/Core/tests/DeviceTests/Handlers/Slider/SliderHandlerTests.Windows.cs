using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SliderHandlerTests
	{
		[Fact(DisplayName = "ThumbImageSource Initializes Correctly")]
		public async Task ThumbImageSourceInitializesCorrectly()
		{
			var slider = new SliderStub()
			{
				ThumbImageSource = new FileImageSourceStub("red.png")
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SliderHandler>(slider);
				await Task.Delay(1000);
				await handler.PlatformView.AssertContainsColor(Colors.Red);
			});
		}

		Slider GetNativeSlider(SliderHandler sliderHandler) =>
			sliderHandler.PlatformView;

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Value;

		double GetNativeMinimum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Minimum;

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Maximum;
	}
}
