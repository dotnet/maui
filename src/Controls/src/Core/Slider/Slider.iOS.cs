using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class Slider
	{
		internal static void MapUpdateOnTap(SliderHandler handler, Slider slider) =>
			MapUpdateOnTap((ISliderHandler)handler, slider);

		internal static void MapUpdateOnTap(ISliderHandler handler, Slider slider)
		{
			if (handler is SliderHandler sliderHandler)
			{
				sliderHandler.MapUpdateOnTap(slider.OnThisPlatform().GetUpdateOnTap());
			}
		}
	}
}