using System;
using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SliderHandlerTests
	{
		[Fact(DisplayName = "ThumbImageSource Initializes Correctly", Skip = "There seems to be an issue, so disable for now: https://github.com/dotnet/maui/issues/1275")]
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

		SeekBar GetNativeSlider(SliderHandler sliderHandler) =>
			sliderHandler.PlatformView;

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Progress;

		double GetNativeMinimum(SliderHandler sliderHandler)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(26))
			{
				return GetNativeSlider(sliderHandler).Min;
			}

			return 0;
		}

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Max;

		Task ValidateNativeThumbColor(ISlider slider, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				return GetNativeSlider(CreateHandler(slider)).AssertContainsColorAsync(color);
			});
		}

		[Fact(DisplayName = "Maximum Value Initializes Correctly")]
		public async Task MaximumInitializesCorrectly()
		{
			var xplatMaximum = 1;
			var slider = new SliderStub()
			{
				Maximum = xplatMaximum
			};

			double expectedValue = SliderExtensions.PlatformMaxValue;

			var values = await GetValueAsync(slider, (handler) =>
			{
				return new
				{
					ViewValue = slider.Maximum,
					PlatformViewValue = GetNativeMaximum(handler)
				};
			});

			Assert.Equal(xplatMaximum, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Value Initializes Correctly")]
		public async Task ValueInitializesCorrectly()
		{
			var xplatValue = 0.5;
			var slider = new SliderStub()
			{
				Maximum = 1,
				Minimum = 0,
				Value = xplatValue
			};

			int expectedValue = (int)(SliderExtensions.PlatformMaxValue / 2);

			var values = await GetValueAsync(slider, (handler) =>
			{
				return new
				{
					ViewValue = slider.Value,
					PlatformViewValue = GetNativeProgress(handler)
				};
			});

			Assert.Equal(xplatValue, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}
	}
}