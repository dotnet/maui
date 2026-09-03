#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		internal static void MapBarBackground(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapBarBackgroundColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapBarTextColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapUnselectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapSelectedTabColor(ITabbedViewHandler handler, TabbedPage view)
		{
		}

		internal static void MapItemsSource(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapItemTemplate(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapSelectedItem(ITabbedViewHandler handler, TabbedPage view)
		{
		}
		internal static void MapCurrentPage(ITabbedViewHandler handler, TabbedPage view)
		{

		}

		internal static void MapPrefersHomeIndicatorAutoHiddenProperty(ITabbedViewHandler handler, TabbedPage view)
		{
			view.CurrentPage.Handler.UpdateValue(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty));
		}

		internal static void MapPrefersPrefersStatusBarHiddenProperty(ITabbedViewHandler handler, TabbedPage view)
		{
			view.CurrentPage.Handler.UpdateValue(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty));
		}
	}
}
