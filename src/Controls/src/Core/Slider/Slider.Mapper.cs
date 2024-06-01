using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Slider
	{
		[Obsolete("Use SliderHandler.Mapper instead.")]
		public static IPropertyMapper<ISlider, SliderHandler>? ControlsSliderMapper = new ControlsMapper<Slider, SliderHandler>(SliderHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Slider legacy behaviors
#if IOS
			SliderHandler.Mapper.ReplaceMapping<Slider, ISliderHandler>(PlatformConfiguration.iOSSpecific.Slider.UpdateOnTapProperty.PropertyName, MapUpdateOnTap);
#endif
		}
	}
}