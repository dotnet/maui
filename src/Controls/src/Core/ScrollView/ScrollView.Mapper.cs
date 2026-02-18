#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		static ScrollView()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(ScrollView), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			// Adjust the mappings to preserve Controls.ScrollView legacy behaviors
#if IOS
			ScrollViewHandler.Mapper.ReplaceMapping<ScrollView, IScrollViewHandler>(PlatformConfiguration.iOSSpecific.ScrollView.ShouldDelayContentTouchesProperty.PropertyName, MapShouldDelayContentTouches);
#endif
		}
	}
}
