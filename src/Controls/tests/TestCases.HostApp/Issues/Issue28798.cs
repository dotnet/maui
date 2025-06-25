namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 28798, "Controls Disappear When WebView is Used with Hardware Acceleration Disabled in Android", PlatformAffected.Android)]
public partial class Issue28798 : ContentPage
{
	public Issue28798()
	{
		var grid = new Grid
		{
			HeightRequest = 500,
			Background = Colors.Green,
			WidthRequest = 300
		};

		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		var label = new Label
		{
			Text = "Test",
			Background = Colors.Red,
			AutomationId = "TestLabel"
		};
		Grid.SetRow(label, 0);
		grid.Children.Add(label);

		var button = new Button
		{
			Text = "button",
			Background = Colors.Blue
		};
		Grid.SetRow(button, 1);
		grid.Children.Add(button);

		var webView = new WebView
		{
			Source = "https://example.com/",
			BackgroundColor = Colors.Transparent,
			HeightRequest = 300,
			WidthRequest = 300
		};
		Grid.SetRow(webView, 2);
		grid.Children.Add(webView);

		Content = grid;
	}
}
