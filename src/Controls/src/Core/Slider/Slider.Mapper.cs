using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Slider
	{
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(Slider)))
			{
				base.RemapForControls(remapped);

				// Adjust the mappings to preserve Controls.Slider legacy behaviors
#if IOS
				SliderHandler.Mapper.ReplaceMapping<Slider, ISliderHandler>(PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty.PropertyName, MapUpdateOnTap);
#endif
			}
		}
	}
}