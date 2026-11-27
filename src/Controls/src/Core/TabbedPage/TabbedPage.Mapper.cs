#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		internal new static void RemapForControls()
		{
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(BarBackground), MapBarBackground);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(BarBackgroundColor), MapBarBackgroundColor);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(BarTextColor), MapBarTextColor);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(UnselectedTabColor), MapUnselectedTabColor);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(SelectedTabColor), MapSelectedTabColor);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(MultiPage<TabbedPage>.ItemsSource), MapItemsSource);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(MultiPage<TabbedPage>.ItemTemplate), MapItemTemplate);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(MultiPage<TabbedPage>.SelectedItem), MapSelectedItem);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(CurrentPage), MapCurrentPage);
#if ANDROID
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(PlatformConfiguration.AndroidSpecific.TabbedPage.IsSwipePagingEnabledProperty.PropertyName, MapIsSwipePagingEnabled);

#pragma warning disable CS0612, CS0618 // Type or member is obsolete
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(PlatformConfiguration.AndroidSpecific.TabbedPage.OffscreenPageLimitProperty.PropertyName, MapOffscreenPageLimit);
#pragma warning restore CS0612, CS0618 // Type or member is obsolete

#endif

#if PLATFORM
			TabbedViewHandler.PlatformViewFactory = OnCreatePlatformView;
#endif

#if IOS || MACCATALYST
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(nameof(FlowDirection), MapFlowDirection);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName, MapPrefersHomeIndicatorAutoHiddenProperty);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName, MapPrefersPrefersStatusBarHiddenProperty);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty.PropertyName, MapPreferredStatusBarUpdateAnimation);
			TabbedViewHandler.Mapper.ReplaceMappingForControls<TabbedPage, ITabbedViewHandler>(PlatformConfiguration.iOSSpecific.TabbedPage.TranslucencyModeProperty.PropertyName, MapTranslucencyMode);
#endif
		}
	}
}
