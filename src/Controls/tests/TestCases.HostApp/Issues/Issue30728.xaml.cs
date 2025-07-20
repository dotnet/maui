using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 30728, "[Android] Image randomly disappears while switching tabs due to a race condition", PlatformAffected.Android)]
public partial class Issue30728 : ContentPage
{
	public Issue30728()
	{
		InitializeComponent();
	}

	private void OnTab1Clicked(object sender, EventArgs e)
	{
		mainContent.Content = CreateImageGrid("Tab 1 Content", Colors.Red);
	}

	private void OnTab2Clicked(object sender, EventArgs e)
	{
		mainContent.Content = CreateImageGrid("Tab 2 Content", Colors.Blue);
	}

	private void OnTab3Clicked(object sender, EventArgs e)
	{
		mainContent.Content = CreateImageGrid("Tab 3 Content", Colors.Green);
	}

	private Grid CreateImageGrid(string title, Color backgroundColor)
	{
		var grid = new Grid
		{
			BackgroundColor = backgroundColor,
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		var titleLabel = new Label
		{
			Text = title,
			FontSize = 20,
			HorizontalOptions = LayoutOptions.Center,
			TextColor = Colors.White,
			AutomationId = "TabTitle"
		};

		var imagesLayout = new StackLayout
		{
			Orientation = StackOrientation.Vertical,
			Spacing = 10,
			Padding = 20
		};

		// Add multiple images to increase the likelihood of reproducing the race condition
		for (int i = 0; i < 6; i++)
		{
			var image = new Image
			{
				Source = "dotnet_bot.png", // Use a standard image that should be available
				HeightRequest = 100,
				WidthRequest = 100,
				Aspect = Aspect.AspectFit,
				AutomationId = $"TestImage{i}"
			};
			imagesLayout.Children.Add(image);
		}

		grid.Children.Add(titleLabel);
		grid.Children.Add(imagesLayout);
		Grid.SetRow(titleLabel, 0);
		Grid.SetRow(imagesLayout, 1);

		return grid;
	}
}