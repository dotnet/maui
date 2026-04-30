using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14708, "Android SearchBar in landscape shows full-screen IME extract mode", PlatformAffected.Android)]
public class Issue14708 : ContentPage
{
	Label _searchTextLabel;
	SearchBar _primarySearchBar;

	public Issue14708()
	{
		_searchTextLabel = new Label
		{
			Text = "Search text: (none)",
			AutomationId = "SearchTextLabel",
			FontSize = 13
		};

		_primarySearchBar = new SearchBar
		{
			Text = "Hello, landscape!",
			Placeholder = "Tap here in landscape — keyboard should be inline",
			AutomationId = "SearchBarControl"
		};

		var searchBar2 = new SearchBar
		{
			Placeholder = "Second SearchBar",
			AutomationId = "SearchBar2"
		};


		var searchBar3 = new SearchBar
		{
			Text = "Hello, landscape!",
			Placeholder = "Third SearchBar",
			AutomationId = "SearchBar3"
		};


		Content = new ScrollView
		{
			VerticalOptions = LayoutOptions.FillAndExpand,
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(16),
				Spacing = 12,
				Children =
				{
					new Label
					{
						Text = "Rotate to LANDSCAPE, then tap any SearchBar. " +
							   "The keyboard should appear inline at the bottom — " +
							   "NOT as a full-screen black overlay.",
						HorizontalTextAlignment = TextAlignment.Center
					},
					_primarySearchBar,
					_searchTextLabel,
					new BoxView { HeightRequest = 1, Color = Colors.LightGray },
					new Label { Text = "Additional SearchBars:", FontAttributes = FontAttributes.Italic, FontSize = 13 },
					searchBar2,
					searchBar3
				}
			}
		};
	}

}
