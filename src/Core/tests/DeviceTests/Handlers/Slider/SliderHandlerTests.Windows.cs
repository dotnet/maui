using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SliderHandlerTests
	{
		// https://github.com/dotnet/maui/issues/12405
		[Theory(DisplayName = "Platform Slider SmallChange Initializes Correctly")]
		[InlineData(0, 1, 0)]
		[InlineData(0, 1, 0.5)]
		[InlineData(0, 1, 1)]
		[InlineData(0, 100, 0)]
		[InlineData(0, 100, 1)]
		[InlineData(0, 100, 5)]
		[InlineData(0, 100, 50)]
		[InlineData(0, 100, 100)]
		[InlineData(0, 100, 10000)]
		[InlineData(0, 100, -10000)]
		[InlineData(0, 10000, 10000)]
		[InlineData(0, 10000, -10000)]
		public async Task SmallChangeInitializesCorrectly(double min, double max, double value)
		{
			var slider = new SliderStub()
			{
				Maximum = max,
				Minimum = min,
				Value = value
			};

			var expected = await GetValueAsync(slider, GetSmallChange);

			Assert.True(expected != 0);
		}

		Slider GetNativeSlider(SliderHandler sliderHandler) =>
			sliderHandler.PlatformView;

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Value;

		double GetNativeMinimum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Minimum;

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Maximum;

		double GetSmallChange(SliderHandler sliderHandler) =>
		GetNativeSlider(sliderHandler).SmallChange;
	}
}