using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, WindowHeader>
	{
		NavigationRootManager? NavigationRootManager =>
			MauiContext?.GetNavigationRootManager();

		protected override WindowHeader CreateNativeElement()
		{
			if (NavigationRootManager?.RootView is NavigationView nv &&
				nv.Header is WindowHeader windowHeader)
			{
				windowHeader.NavigationView = nv as MauiNavigationView;
				return windowHeader;
			}

			return new WindowHeader()
			{
				NavigationView = NavigationRootManager?.RootView as MauiNavigationView
			};
		}
	}
}
