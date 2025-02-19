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

		Flyout = new Page { Title = "Flyout" };
		Detail = new SplashPage();
	}

	public static void SwitchDetail()
	{
		MDP.Detail = new ContentPage { Content = new Label { Text = "If this did not crash, this test has passed." }, Padding = 20 };
	}
}