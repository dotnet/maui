namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29516, "Issue with the TitleBar TrailingContent not being properly aligned on macOS", PlatformAffected.macOS)]
public partial class Issue29516 : ContentPage
{
	TitleBar _titleBar;
	public Issue29516()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = "This is a test for the TitleBar TrailingContent alignment issue on macOS.",
					AutomationId="ContentLabel",
				}
			}
		};
		_titleBar = new TitleBar
		{
			Title = ".NET MAUI",
			BackgroundColor = Colors.Blue,
			HeightRequest = 50,
			Content = new SearchBar
			{
				Placeholder = "TitleBar Content",
				PlaceholderColor = Colors.White,
				BackgroundColor = Colors.Blue,
				MaximumWidthRequest = 300,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Center
			},
			TrailingContent = new HorizontalStackLayout
			{
				Spacing = 8,
				Margin = new Thickness(4),
				Children =
			{
				new Border
				{
					Content = new StackLayout
					{
						VerticalOptions = LayoutOptions.Center,
						BackgroundColor = Colors.Red,
						Children =
						{
							new Label { Text = "TrailingContent" }
						}
					}
				}
			}
			}
		};

	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (Window is not null)
		{
			Window.TitleBar = _titleBar;
		}
	}
}