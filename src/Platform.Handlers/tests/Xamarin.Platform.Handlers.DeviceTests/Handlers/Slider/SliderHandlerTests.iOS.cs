using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xunit;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class SliderHandlerTests
	{
		UISlider GetNativeSlider(SliderHandler sliderHandler) =>
			(UISlider)sliderHandler.View;

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).Value;

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetNativeSlider(sliderHandler).MaxValue;

		async Task ValidateNativeThumbColor(ISlider slider, Color color)
		{
			var expected = await GetValueAsync(slider, handler => GetNativeSlider(handler).ThumbTintColor.ToColor());
			Assert.Equal(expected, color);
		}
	}
}
