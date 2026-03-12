using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Slider
	{
		static Slider()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
			RemappingHelper.EnsureBaseTypeRemapped(typeof(Slider), typeof(VisualElement));

			// Adjust the mappings to preserve Controls.Slider legacy behaviors
#if IOS
			SliderHandler.Mapper.ReplaceMapping<Slider, ISliderHandler>(PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty.PropertyName, MapUpdateOnTap);
#endif
		}
	}
}