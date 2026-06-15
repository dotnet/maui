#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(NavigationPage)))
			{
				base.RemapForControls(remapped);

				// Adjust the mappings to preserve Controls.NavigationPage legacy behaviors
#if IOS
				NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName, MapPrefersLargeTitles);
#endif
			}
		}
	}
}
