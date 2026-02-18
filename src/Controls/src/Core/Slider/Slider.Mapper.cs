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
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(Slider), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			// Adjust the mappings to preserve Controls.Slider legacy behaviors
#if IOS
			SliderHandler.Mapper.ReplaceMapping<Slider, ISliderHandler>(PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty.PropertyName, MapUpdateOnTap);
#endif
		}
	}
}