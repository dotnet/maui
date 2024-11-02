using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24489, "TitleBar Implementation", PlatformAffected.macOS)]

public partial class Issue24489 : ContentPage
{
	TitleBar _customTitleBar;

	public Issue24489()
	{
		InitializeComponent();
		_customTitleBar = new TitleBar()
		{
			Title = "Title",
			Subtitle = "Subtitle",
			BackgroundColor = Colors.Blue,
			ForegroundColor = Colors.White,
			Icon = "dotnet_bot.png",
			Content = new Label()
			{
				Text = "Have a nice day! :)",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				Background = Colors.LightBlue,
				TextColor = Colors.Black,
			},
			HeightRequest = 32,
			TrailingContent = new Border()
			{
				WidthRequest = 32,
				HeightRequest = 32,
				StrokeShape = new Ellipse() { WidthRequest = 32, HeightRequest = 32 },
				StrokeThickness = 0,
				BackgroundColor = Colors.Azure,
				Content = new Label()
				{
					Text = "TJ",
					TextColor = Colors.Black,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 10
				}
			},
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Window.TitleBar = _customTitleBar;
	}

	void ToggleTitleBarButtonClicked(object sender, EventArgs e)
	{
		if (Window.TitleBar is TitleBar tbar)
		{
			tbar.IsVisible = !tbar.IsVisible;
		}
	}
}
