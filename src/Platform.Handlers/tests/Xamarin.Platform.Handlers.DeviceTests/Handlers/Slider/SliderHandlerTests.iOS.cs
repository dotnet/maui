
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xunit;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class SliderHandlerTests
	{
		UISlider GetSlider(SliderHandler sliderHandler) =>
			((UISlider)sliderHandler.View);

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetSlider(sliderHandler).Value;

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetSlider(sliderHandler).MaxValue;


		async Task ValidateNativeThumbColor(ISlider slider, Color color)
		{
			var expected = await GetValueAsync(slider, handler => GetSlider(handler).ThumbTintColor.ToColor());
			Assert.Equal(expected, color);
		}
	}
}
