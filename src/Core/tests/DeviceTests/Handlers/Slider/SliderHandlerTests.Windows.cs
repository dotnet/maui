using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SliderHandlerTests
	{
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
