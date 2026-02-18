#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class ContentPage
	{
		static ContentPage()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
RemappingHelper.EnsureBaseTypeRemapped(typeof(ContentPage), typeof(VisualElement));

			PageHandler.Mapper.ReplaceMapping<ContentPage, IPageHandler>(nameof(ContentPage.HideSoftInputOnTapped), MapHideSoftInputOnTapped);
#if IOS
			PageHandler.Mapper.ReplaceMapping<ContentPage, IPageHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName, MapPrefersHomeIndicatorAutoHidden);
			PageHandler.Mapper.ReplaceMapping<ContentPage, IPageHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName, MapPrefersStatusBarHidden);
#endif
		}

#if IOS
		static void MapPrefersHomeIndicatorAutoHidden(IPageHandler handler, ContentPage page)
		{
			handler?.UpdateValue(nameof(IiOSPageSpecifics.IsHomeIndicatorAutoHidden));
		}

		static void MapPrefersStatusBarHidden(IPageHandler handler, ContentPage page)
		{
			handler?.UpdateValue(nameof(IiOSPageSpecifics.PrefersStatusBarHiddenMode));
		}
#endif

		static void MapHideSoftInputOnTapped(IPageHandler handler, ContentPage page)
		{
			page.UpdateHideSoftInputOnTapped();
		}

		void UpdateHideSoftInputOnTapped()
		{
			Handler
				?.GetService<HideSoftInputOnTappedChangedManager>()
				?.UpdatePage(this);
		}
	}
}
