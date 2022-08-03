using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class TabbedPage
	{
		public static IPropertyMapper<ITabbedView, ITabbedViewHandler> ControlsTabbedPageMapper = new PropertyMapper<TabbedPage, ITabbedViewHandler>(TabbedViewHandler.Mapper)
		{
			[nameof(BarBackground)] = MapBarBackground,
			[nameof(BarBackgroundColor)] = MapBarBackgroundColor,
			[nameof(BarTextColor)] = MapBarTextColor,
			[nameof(UnselectedTabColor)] = MapUnselectedTabColor,
			[nameof(SelectedTabColor)] = MapSelectedTabColor,
			[nameof(MultiPage<TabbedPage>.ItemsSource)] = MapItemsSource,
			[nameof(MultiPage<TabbedPage>.ItemTemplate)] = MapItemTemplate,
			[nameof(MultiPage<TabbedPage>.SelectedItem)] = MapSelectedItem,
			[nameof(CurrentPage)] = MapCurrentPage,
#if ANDROID
			[PlatformConfiguration.AndroidSpecific.TabbedPage.IsSwipePagingEnabledProperty.PropertyName] = MapIsSwipePagingEnabled
#endif
		};

		internal new static void RemapForControls()
		{
			TabbedViewHandler.Mapper = ControlsTabbedPageMapper;

#if WINDOWS || ANDROID
			TabbedViewHandler.PlatformViewFactory = OnCreatePlatformView;
#endif
		}
	}
}
