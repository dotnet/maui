using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
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
				await handler.NativeView.AssertContainsColor(Colors.Red);
			});
		}

		UISlider GetNativeSlider(SliderHandler sliderHandler) =>
			sliderHandler.NativeView;

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Value;

		double GetNativeMinimum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).MinValue;

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).MaxValue;

		async Task ValidateNativeThumbColor(ISlider slider, Color color)
		{
			var expected = await GetValueAsync(slider, handler => GetNativeSlider(handler).ThumbTintColor.ToColor());
			Assert.Equal(expected, color);
		}
	}
}