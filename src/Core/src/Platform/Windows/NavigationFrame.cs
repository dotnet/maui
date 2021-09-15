using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class NavigationFrame : Frame
	{
		// This is the best way I've found to get the NavigationManager to the currently activating Page.
		// OnNavigatedTo inside Page fires before the page has loaded. We can then access Page.Frame to retrieve the
		// NavigationManager and get the IView that needs to be displayed. Another way to do this would be to use
		// parameters but we can only pass primitives as parameters which then seems to cause the same problem. Unless
		// we create some static property to load the data from but that feels dangerous.
		public NavigationManager NavigationManager { get; }

		public NavigationFrame(NavigationManager navigationManager)
		{
			NavigationManager = navigationManager;
		}
	}
}