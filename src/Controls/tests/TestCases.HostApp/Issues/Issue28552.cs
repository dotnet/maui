namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28552, "Changing StatusBar and NavigationBar background color doesn't work with Modal pages", PlatformAffected.Android)]
public class Issue28552 : ContentPage
{
	public Issue28552()
	{
		Content = new Button()
		{
			Text = "Click to Open Modal",
			AutomationId = "OpenModalButton",
			Command = new Command(() =>
			{
#if ANDROID
					Android.Views.Window window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.Window;
#pragma warning disable CA1422
					window.SetNavigationBarColor(Android.Graphics.Color.Purple);
					window.SetStatusBarColor(Android.Graphics.Color.Green);
#endif
				Window!.Page!.Navigation.PushModalAsync(new ContentPage
				{
					Content = new Label
					{
						VerticalOptions = LayoutOptions.Center,
						Text = "Hello from Maui!",
						AutomationId = "LabelOnModalPage"
					}
				}, false);
			})
		};
	}
}