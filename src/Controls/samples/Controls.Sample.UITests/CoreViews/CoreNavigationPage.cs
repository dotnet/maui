using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	internal class CoreNavigationPage : Microsoft.Maui.Controls.NavigationPage
	{
		public CoreNavigationPage()
		{
			AutomationId = "NavigationPageRoot";

			BarBackgroundColor = Colors.Maroon;
			BarTextColor = Colors.Yellow;

			On<iOS>().SetPrefersLargeTitles(true);

			Navigation.PushAsync(new CoreRootPage(this));
		}
	}
}