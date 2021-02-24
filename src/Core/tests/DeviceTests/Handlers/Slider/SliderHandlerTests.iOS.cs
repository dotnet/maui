using Microsoft.Maui.Handlers;
using System.Threading.Tasks;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SliderHandlerTests
	{
		UISlider GetNativeSlider(SliderHandler sliderHandler) =>
			(UISlider)sliderHandler.View;

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
