#if IOS
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Application = Microsoft.Maui.Controls.Application;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23411, "[iOS]Use non-overridden traits in AppInfoImplementation.RequestTheme",
		PlatformAffected.iOS)]
	public partial class Issue23411 : ContentPage
	{
		public Issue23411()
		{
			InitializeComponent();
			Application.Current.UserAppTheme = AppTheme.Dark;
		}

		private void OnModalPage(object sender, EventArgs e)
		{
			var modalPage = new NavigationPage(new Issue23411() { Title = "Modal Page" ,AutomationId = "PageSheetModalPage" });
			modalPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
			Navigation.PushModalAsync(modalPage);
		}

		private void OnDarkTheme(object sender, EventArgs e)
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
		}

		private void OnLightTheme(object sender, EventArgs e)
		{
			Application.Current!.UserAppTheme = AppTheme.Light;
		}

		private void OnResetTheme(object sender, EventArgs e)
		{
			Application.Current!.UserAppTheme = AppTheme.Unspecified;
		}
	}
}
#endif
