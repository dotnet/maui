#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		[Obsolete("Use TabbedViewHandler.Mapper instead.")]
		public static IPropertyMapper<ITabbedView, ITabbedViewHandler> ControlsTabbedPageMapper = new ControlsMapper<TabbedPage, ITabbedViewHandler>(TabbedViewHandler.Mapper);

		internal new static void RemapForControls()
		{
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(BarBackground), MapBarBackground);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(BarBackgroundColor), MapBarBackgroundColor);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(BarTextColor), MapBarTextColor);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(UnselectedTabColor), MapUnselectedTabColor);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(SelectedTabColor), MapSelectedTabColor);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(MultiPage<TabbedPage>.ItemsSource), MapItemsSource);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(MultiPage<TabbedPage>.ItemTemplate), MapItemTemplate);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(MultiPage<TabbedPage>.SelectedItem), MapSelectedItem);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(CurrentPage), MapCurrentPage);
#if ANDROID
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(PlatformConfiguration.AndroidSpecific.TabbedPage.IsSwipePagingEnabledProperty.PropertyName, MapIsSwipePagingEnabled);
#endif

#if WINDOWS || ANDROID || TIZEN
			TabbedViewHandler.PlatformViewFactory = OnCreatePlatformView;
#endif

#if IOS
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty), MapPrefersHomeIndicatorAutoHiddenProperty);
			TabbedViewHandler.Mapper.ReplaceMapping<TabbedPage, ITabbedViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty), MapPrefersPrefersStatusBarHiddenProperty);
#endif
		}
	}
}
