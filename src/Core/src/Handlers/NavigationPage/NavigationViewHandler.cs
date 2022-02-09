using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler
	{
		public static PropertyMapper<IStackNavigationView, NavigationViewHandler> NavigationViewMapper
			   = new PropertyMapper<IStackNavigationView, NavigationViewHandler>(ViewHandler.ViewMapper);

		public static CommandMapper<IStackNavigationView, NavigationViewHandler> NavigationViewCommandMapper = new(ViewCommandMapper)
		{
			[nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
		};

		public NavigationViewHandler() : base(NavigationViewMapper, NavigationViewCommandMapper)
		{
		}

		public NavigationViewHandler(PropertyMapper? mapper = null) : base(mapper ?? NavigationViewMapper, NavigationViewCommandMapper)
		{

		}
	}
}
