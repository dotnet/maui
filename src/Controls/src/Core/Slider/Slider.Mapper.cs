using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Slider
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.Slider legacy behaviors
#if IOS
			SliderHandler.Mapper.ReplaceMappingForControls<Slider, ISliderHandler>(PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty.PropertyName, MapUpdateOnTap);
#endif
		}
	}
}