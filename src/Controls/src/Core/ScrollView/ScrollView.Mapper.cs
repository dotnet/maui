#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		[Obsolete("Use ScrollViewHandler.Mapper instead.")]
		public static IPropertyMapper<IScrollView, ScrollViewHandler> ControlsScrollViewMapper =
				new ControlsMapper<ScrollView, ScrollViewHandler>(ScrollViewHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.ScrollView legacy behaviors
#if IOS
			ScrollViewHandler.Mapper.ReplaceMapping<ScrollView, IScrollViewHandler>(PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty.PropertyName, MapShouldDelayContentTouches);
#endif
		}
	}
}
