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
		internal new static void RemapForControls()
		{
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
