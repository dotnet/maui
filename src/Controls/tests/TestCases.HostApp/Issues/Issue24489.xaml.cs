using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24489, "TitleBar Implementation", PlatformAffected.macOS)]

public partial class Issue24489 : ContentPage
{
	TitleBar _customTitleBar;

	public Issue24489()
	{
		InitializeComponent();
		SetTitleBar();
	}

	public Issue24489(int heightRequest) : this()
	{
		_customTitleBar.HeightRequest = heightRequest;
	}

	public Issue24489(bool isEmptyTitleBar)
	{
		InitializeComponent();

		if (isEmptyTitleBar)
		{
			_customTitleBar = new TitleBar();
		}
	}

	void SetTitleBar()
	{
		var grid = CreateGrid(true);

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
		if (Window is not null)
		{
			Window.TitleBar = _customTitleBar;
		}
		else if (Shell.Current?.Window is not null)
		{
			Shell.Current.Window.TitleBar = _customTitleBar;
		}
	}

	Grid CreateGrid(bool isInitial)
	{
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
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End
		};

		firstLabel.Text = isInitial ? "Hello World!" : "New Hello World!";

		var vsl = new VerticalStackLayout()
		{
			Children = { firstLabel },
			BackgroundColor = Colors.LightGreen,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End
		};

		var secondLabel = new Label
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End
		};

		secondLabel.Text = isInitial ? "Have a nice day!" : "New have a nice day!";

		Grid.SetColumn(vsl, 0);
		grid.Children.Add(vsl);
		Grid.SetColumn(secondLabel, 1);
		grid.Children.Add(secondLabel);

		return grid;
	}

	void ToggleTitleBarButtonClicked(object sender, EventArgs e)
	{
		if (Window.TitleBar is TitleBar tbar)
		{
			tbar.IsVisible = !tbar.IsVisible;
		}
	}

	void ToggleButtonClicked2(object sender, EventArgs e)
	{
		if (Window.TitleBar is TitleBar tbar)
		{
			tbar.Title = tbar.Title == "Title" ? "New Title" : "Title";
			tbar.Subtitle = tbar.Subtitle == "Subtitle" ? "New Subtitle" : "Subtitle";
			tbar.HeightRequest = tbar.HeightRequest == 300 ? 400 : 300;

			if (tbar.Title == "Title")
			{
				tbar.Content = CreateGrid(true);
			}
			else
			{
				tbar.Content = CreateGrid(false);
			}
		}
	}
}
