using System;
using System.Threading;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Slider
	{
		static int s_remappedForControls;

		internal new static void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			VisualElement.RemapForControls();

			// Adjust the mappings to preserve Controls.Slider legacy behaviors
#if IOS
			SliderHandler.Mapper.ReplaceMapping<Slider, ISliderHandler>(PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty.PropertyName, MapUpdateOnTap);
#endif
		}
	}
}