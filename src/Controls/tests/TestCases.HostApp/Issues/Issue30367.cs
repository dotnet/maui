namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30367, "SearchBar FlowDirection Property Not Working on Android", PlatformAffected.Android)]
public class Issue30367 : ContentPage
{
	SearchBar ltrSearchBar;
	SearchBar rtlSearchBar;

	public Issue30367()
	{
		Title = "Issue 30367";

		ltrSearchBar = new SearchBar
		{
			Placeholder = "Search here...",
			Text = "Left to Right",
			BackgroundColor = Colors.LightBlue,
			FlowDirection = FlowDirection.LeftToRight
		};

		rtlSearchBar = new SearchBar
		{
			Placeholder = "Search here...",
			Text = "Right to Left",
			BackgroundColor = Colors.LightBlue,
			FlowDirection = FlowDirection.RightToLeft
		};

		Content = new StackLayout
		{
			Children =
			{
				new Label { Text = "LTR SearchBar:", AutomationId = "SearchBarLabel" },
				ltrSearchBar,
				new Label { Text = "RTL SearchBar:" },
				rtlSearchBar,
			}
		};
	}
}