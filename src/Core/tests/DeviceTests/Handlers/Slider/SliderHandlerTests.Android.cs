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
		SeekBar GetNativeSlider(SliderHandler sliderHandler) =>
			(SeekBar)sliderHandler.NativeView;

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Progress;

		double GetNativeMinimum(SliderHandler sliderHandler)
		{
			if (NativeVersion.Supports(NativeApis.SeekBarSetMin))
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
				return GetNativeSlider(CreateHandler(slider)).AssertContainsColor(color);
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

			double expectedValue = SliderExtensions.NativeMaxValue;

			var values = await GetValueAsync(slider, (handler) =>
			{
				return new
				{
					ViewValue = slider.Maximum,
					NativeViewValue = GetNativeMaximum(handler)
				};
			});

			Assert.Equal(xplatMaximum, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
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

			int expectedValue = (int)(SliderExtensions.NativeMaxValue / 2);

			var values = await GetValueAsync(slider, (handler) =>
			{
				return new
				{
					ViewValue = slider.Value,
					NativeViewValue = GetNativeProgress(handler)
				};
			});

			Assert.Equal(xplatValue, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}
	}
}