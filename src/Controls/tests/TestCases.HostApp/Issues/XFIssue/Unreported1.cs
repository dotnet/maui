namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "NRE when switching page on Appearing", PlatformAffected.iOS)]
public class Unreported1 : TestFlyoutPage
{
	static Unreported1 MDP;

	class SplashPage : ContentPage
	{
		protected override void OnAppearing()
		{
			base.OnAppearing();

			// You really shouldn't do this, but we shouldn't crash when you do it. :)
			SwitchDetail();
		}
	}

	protected override void Init()
	{
		MDP = this;
		// When assgining a page getting an invalid cast exception in all platforms, so using content page here. //Issue: https://github.com/dotnet/maui/issues/21205
		Flyout = new ContentPage { Title = "Flyout", Content = new Label { Text = "Flyout Content" } };
		Detail = new SplashPage();
	}

	public static void SwitchDetail()
	{
		MDP.Detail = new ContentPage { Content = new Label { Text = "If this did not crash, this test has passed.", AutomationId = "Label" }, Padding = 20 };
	}
}


