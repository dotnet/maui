using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Handlers
{
	internal partial class NavigationPageHandler
	{
		public static IPropertyMapper<INavigationView, NavigationPageHandler> NavigationPageMapper =
			new PropertyMapper<INavigationView, NavigationPageHandler>(ViewHandler.ViewMapper)
			{
				//[NavigationPage.BarTextColorProperty.PropertyName] = MapBarTextColor,
				//[NavigationPage.BarBackgroundColorProperty.PropertyName] = MapBarBackground,
				//[NavigationPage.BarBackgroundProperty.PropertyName] = MapBarBackground,
				//[nameof(IPadding.Padding)] = MapPadding,
				//[nameof(NavigationPage.TitleIconImageSourceProperty.PropertyName)] = MapTitleIcon,
				//[nameof(NavigationPage.TitleViewProperty.PropertyName)] = MapTitleView,

#if WINDOWS
				//[nameof(ToolbarPlacementProperty.PropertyName)] = MapToolbarPlacement,
				//[nameof(ToolbarDynamicOverflowEnabledProperty.PropertyName)] = MapToolbarDynamicOverflowEnabled,
#endif
			};

		public static CommandMapper<INavigationView, NavigationPageHandler> NavigationViewCommandMapper = new(ViewCommandMapper)
		{
			[nameof(INavigationView.PushAsync)] = PushAsyncTo,
			[nameof(INavigationView.PopAsync)] = PopAsyncTo
		};

		public NavigationPageHandler() : base(NavigationPageMapper, NavigationViewCommandMapper)
		{
		}

		public NavigationPageHandler(IPropertyMapper? mapper = null) : base(mapper ?? NavigationPageMapper, NavigationViewCommandMapper)
		{

		}
	}
}
