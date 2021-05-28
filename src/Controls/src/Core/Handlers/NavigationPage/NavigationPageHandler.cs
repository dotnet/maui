using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

#if WINDOWS
using static Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Page;
#endif

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler
	{
		public static PropertyMapper<NavigationPage, NavigationPageHandler> NavigationPageMapper
			   = new PropertyMapper<NavigationPage, NavigationPageHandler>(ViewHandler.ViewMapper)
			   {
				   [NavigationPage.BarTextColorProperty.PropertyName] = MapBarTextColor,
				   [NavigationPage.BarBackgroundColorProperty.PropertyName] = MapBarBackground,
				   [NavigationPage.BarBackgroundProperty.PropertyName] = MapBarBackground,
				   [nameof(IPadding.Padding)] = MapPadding,
				   [nameof(NavigationPage.TitleIconImageSourceProperty.PropertyName)] = MapTitleIcon,
				   [nameof(NavigationPage.TitleViewProperty.PropertyName)] = MapTitleView,

#if WINDOWS
					[nameof(ToolbarPlacementProperty.PropertyName)] = MapToolbarPlacement,
					[nameof(ToolbarDynamicOverflowEnabledProperty.PropertyName)] = MapToolbarDynamicOverflowEnabled,
#endif
			   };

		public NavigationPageHandler() : base(NavigationPageMapper)
		{
		}
	}
}
