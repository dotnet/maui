using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;


namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler
	{
		public static PropertyMapper<INavigationView, NavigationViewHandler> NavigationViewMapper
			   = new PropertyMapper<INavigationView, NavigationViewHandler>(ViewHandler.ViewMapper);

		public static CommandMapper<INavigationView, NavigationViewHandler> NavigationViewCommandMapper = new(ViewCommandMapper)
		{
			[nameof(INavigationView.RequestNavigation)] = RequestNavigation
		};

		public NavigationViewHandler() : base(NavigationViewMapper, NavigationViewCommandMapper)
		{
		}

		public NavigationViewHandler(PropertyMapper? mapper = null) : base(mapper ?? NavigationViewMapper, NavigationViewCommandMapper)
		{

		}
	}
}
