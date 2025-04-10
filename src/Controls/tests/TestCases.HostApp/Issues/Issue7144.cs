using System.Collections.ObjectModel;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 7144, "IndicatorView using templated icons not working", PlatformAffected.UWP)]
public class Issue7144 : ContentPage
{
	public ObservableCollection<Issue7144Monkey> Monkeys { get; set; } = new ObservableCollection<Issue7144Monkey>();

	public Issue7144()
	{
		CreateMonkeyCollection();

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

		CarouselView carouselViewWithIndicatorView = new CarouselView
		{
			ItemsSource = Monkeys,
			Loop = false,
			IndicatorView = indicatorViewWithDataTemplate,
			ItemTemplate = new DataTemplate(() =>
			{
				StackLayout stackLayout = new StackLayout();

				Label nameLabel = new Label
				{
					FontAttributes = FontAttributes.Bold,
					FontSize = 24,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				nameLabel.SetBinding(Label.TextProperty, "Name");

				Image image = new Image
				{
					Aspect = Aspect.Fill,
					HeightRequest = 150,
					WidthRequest = 150,
					HorizontalOptions = LayoutOptions.Center
				};
				image.SetBinding(Image.SourceProperty, "Image");

				stackLayout.Children.Add(nameLabel);
				stackLayout.Children.Add(image);

				return stackLayout;
			})
		};

		Label label = new Label
		{
			AutomationId = "IndicatorViewWithDataTemplate",
			Text = "The test case fails if the IndicatorView DataTemplate is not displayed",
			FontAttributes = FontAttributes.Bold,
			FontSize = 24,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		grid.Add(carouselViewWithIndicatorView, 0, 0);
		grid.Add(indicatorViewWithDataTemplate, 0, 1);
		grid.Add(label, 0, 2);

		Content = grid;
	}

	void CreateMonkeyCollection()
	{
		Monkeys.Add(new Issue7144Monkey
		{
			Name = "Baboon",
			Image = "dotnet_bot.png"
		});

		Monkeys.Add(new Issue7144Monkey
		{
			Name = "Capuchin Monkey",
			Image = "dotnet_bot.png"
		});
	}
}

public class Issue7144Monkey
{
	public string Name { get; set; }

	public string Image { get; set; }
}


