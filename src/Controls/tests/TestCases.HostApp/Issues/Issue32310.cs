namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32310, "App hangs if PopModalAsync is called after PushModalAsync with single await Task.Yield()", PlatformAffected.Android)]
public class Issue32310 : ContentPage
{
	public Issue32310()
	{
		var navigateButton = new Button
		{
			Text = "Perform Modal Navigation",
			AutomationId = "NavigateButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		navigateButton.Clicked += (s, e) =>
		{
			Dispatcher.DispatchAsync(async () =>
			{
				await Navigation.PushModalAsync(new ContentPage() { Content = new Label() { Text = "Hello!" } }, false);

				await Task.Yield();

				await Navigation.PopModalAsync(false);
			});
		};

		var layout = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				navigateButton
			}
		};

		Content = layout;
	}
}
