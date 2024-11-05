using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24489, "TitleBar Implementation", PlatformAffected.macOS)]

public partial class Issue24489 : ContentPage
{
	TitleBar _customTitleBar;

	public Issue24489()
	{
		InitializeComponent();
		var grid = new Grid
		{
			BackgroundColor = Colors.LightBlue,
			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
				new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
			}
		};

		var firstLabel = new Label
		{
			Text = "Hello, TitleBar!",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End
		};

		var vsl = new VerticalStackLayout()
		{
			Children = { firstLabel },
			BackgroundColor = Colors.LightGreen,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End
		};

		var secondLabel = new Label
		{
			Text = "Have a nice Day!",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End
		};

		Grid.SetColumn(vsl, 0);
		grid.Children.Add(vsl);
		Grid.SetColumn(secondLabel, 1);
		grid.Children.Add(secondLabel);

		_customTitleBar = new TitleBar()
		{
			Title = "Title",
			Subtitle = "Subtitle",
			BackgroundColor = Colors.Blue,
			ForegroundColor = Colors.White,
			Icon = "dotnet_bot.png",
			Content = grid,
			HeightRequest = 300,
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
					VerticalOptions = LayoutOptions.End,
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
