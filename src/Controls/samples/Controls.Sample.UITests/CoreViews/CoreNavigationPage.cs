using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Application = Microsoft.Maui.Controls.Application;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	internal class CoreNavigationPage : Microsoft.Maui.Controls.NavigationPage
	{
		public CoreNavigationPage()
		{
			AutomationId = "NavigationPageRoot";

			InitNavigationPageStyling(this);

			On<iOS>().SetPrefersLargeTitles(true);

			Navigation.PushAsync(new CoreRootPage(this));

			Application.Current.RequestedThemeChanged += UpdateThemeColor;
		}

		private void UpdateThemeColor(object _, AppThemeChangedEventArgs args)
		{
			// Make sure background color is consistent so that the Mica material doesn't cause issues on Windows
			BackgroundColor =
					Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? Colors.Black : Colors.White;
		}

		public static void InitNavigationPageStyling(Microsoft.Maui.Controls.NavigationPage navigationPage)
		{
			navigationPage.BarBackgroundColor = Colors.Maroon;
			navigationPage.BarTextColor = Colors.Yellow;
			navigationPage.BackgroundColor =
					Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? Colors.Black : Colors.White;
		}
	}
}