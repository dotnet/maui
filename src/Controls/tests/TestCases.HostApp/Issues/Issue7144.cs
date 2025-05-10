using System.Collections.ObjectModel;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 7144, "IndicatorView using templated icons not working", PlatformAffected.UWP)]
public class Issue7144 : ContentPage
{
	ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>() { "Item1", "Item2" };

	public Issue7144()
	{
		Grid grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		IndicatorView indicatorViewWithDataTemplate = new IndicatorView
		{
			Margin = new Thickness(0, 0, 0, 40),
			HorizontalOptions = LayoutOptions.Center,
			IndicatorColor = Colors.Black,
			SelectedIndicatorColor = Colors.Green
		};

		indicatorViewWithDataTemplate.IndicatorTemplate = new DataTemplate(() =>
		{
			Image image = new Image()
			{
				Source = "dotnet_bot.png",
				HeightRequest = 15,
				WidthRequest = 15,
			};
			return image;
		});

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = Items,
			Loop = false,
			IndicatorView = indicatorViewWithDataTemplate,
			ItemTemplate = new DataTemplate(() =>
			{
				StackLayout stackLayout = new StackLayout();

				Label label = new Label
				{
					FontAttributes = FontAttributes.Bold,
					FontSize = 24,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				;

				stackLayout.Children.Add(label);

				return stackLayout;
			})
		};

		Label label = new Label
		{
			AutomationId = "descriptionLabel",
			Text = "The test case fails if the IndicatorView DataTemplate is not displayed",
			FontAttributes = FontAttributes.Bold,
			FontSize = 24,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		grid.Add(carouselView, 0, 0);
		grid.Add(indicatorViewWithDataTemplate, 0, 1);
		grid.Add(label, 0, 2);

		Content = grid;
	}
}


