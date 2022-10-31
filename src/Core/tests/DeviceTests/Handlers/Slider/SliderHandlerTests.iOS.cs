using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
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
				await handler.PlatformView.AssertContainsColorAsync(Colors.Red);
			});
		}

		[Theory(DisplayName = "Slider Maximum Text Updates Correctly")]
		[InlineData(0, 1)]
		[InlineData(0, 10)]
		[InlineData(10, 20)]
		public async Task MinimumUpdatesCorrectly(int setValue, int unsetValue)
		{
			var slider = new SliderStub
			{
				Maximum = 100
			};

			await ValidatePropertyUpdatesValue(
				slider,
				nameof(ISlider.Minimum),
				GetNativeMinimum,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "Slider Maximum Text Updates Correctly")]
		[InlineData(10, 20)]
		[InlineData(0, 10)]
		[InlineData(10, 100)]
		public async Task MaximumUpdatesCorrectly(int setValue, int unsetValue)
		{
			var slider = new SliderStub();

			await ValidatePropertyUpdatesValue(
				slider,
				nameof(ISlider.Maximum),
				GetNativeMaximum,
				setValue,
				unsetValue);
		}

		UISlider GetNativeSlider(SliderHandler sliderHandler) =>
			sliderHandler.PlatformView;

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