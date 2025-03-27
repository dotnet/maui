﻿#nullable disable
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
#if ANDROID
			PageHandler.Mapper.ReplaceMapping<ContentPage, IPageHandler>(nameof(ContentPage.InputTransparent), MapInputTransparent);
#elif IOS
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

#if ANDROID
		static void MapInputTransparent(IPageHandler handler, ContentPage page)
		{
			if (handler.PlatformView is ContentViewGroup layout)
			{
				layout.InputTransparent = page.InputTransparent;
			}
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
