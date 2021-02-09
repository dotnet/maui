using System.Threading.Tasks;
using Android.Widget;
using Xamarin.Forms;
using Xunit;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class SliderHandlerTests
	{
		SeekBar GetSlider(SliderHandler sliderHandler) =>
			((SeekBar)sliderHandler.View);

		double GetNativeProgress(SliderHandler sliderHandler) =>
			GetSlider(sliderHandler).Progress;

		double GetNativeMaximum(SliderHandler sliderHandler) =>
			GetSlider(sliderHandler).Max;


		Task ValidateNativeThumbColor(ISlider slider, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetSlider(CreateHandler(slider)).AssertContainsColor(color);
			});
		}
	}
}
