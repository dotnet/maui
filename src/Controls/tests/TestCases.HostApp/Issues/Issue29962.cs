namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29962, "[Windows] SearchBar PlaceHolder and Background Color should update properly at runtime", PlatformAffected.UWP)]
public class Issue29962 : ContentPage
{
	public Issue29962()
	{
		var searchBar = new SearchBar
		{
			Placeholder = "Search here...",
			Margin = new Thickness(0, 30),
			BackgroundColor = Colors.Black,
			PlaceholderColor = Colors.Yellow,
			HeightRequest = 50,
		};
		var button = new Button
		{
			Text = "Change PlaceHolder and Background Color",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "ColorChangeButton"
		};
		button.Clicked += (sender, e) =>
		{
			searchBar.BackgroundColor = Colors.YellowGreen;
			searchBar.PlaceholderColor = Colors.Red;
		};

		Content = new StackLayout
		{
			Children =
			{
				searchBar,
				button
			}
		};
	}
}