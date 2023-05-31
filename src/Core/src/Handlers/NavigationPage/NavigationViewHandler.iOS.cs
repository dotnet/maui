#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, UIView>, IPlatformViewHandler
	{
		public IStackNavigationView NavigationView => ((IStackNavigationView)VirtualView);

		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		protected override UIView CreatePlatformView()
		{
			throw new NotImplementedException("Use Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer");
		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			throw new NotImplementedException("Use Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer");
		}
	}
}
