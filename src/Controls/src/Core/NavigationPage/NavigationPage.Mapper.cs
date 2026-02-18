#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		static NavigationPage()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(NavigationPage), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			// Adjust the mappings to preserve Controls.NavigationPage legacy behaviors
#if IOS
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName, MapPrefersLargeTitles);
#endif
		}
	}
}
