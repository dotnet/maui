using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34783, "CollectionView Dynamic item sizing - After dragging the scrollbar all images return to their original size", PlatformAffected.Android)]
public class Issue34783 : ContentPage
{
	public Issue34783()
	{
		BindingContext = new Issue34783ViewModel();

		var grid = new Grid
		{
			Margin = new Thickness(20),
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		var instructions = new StackLayout
		{
			Children =
			{
				new Label { Text = "1. Confirm that the CollectionView below is populated with monkeys and can be scrolled." },
				new Label { Text = "2. Tap an image to dynamically change its size, then scroll it off-screen and back on-screen. The test passes if the resized image stays resized." }
			}
		};

		var tapLabel = new Label { Text = "Tap each image to dynamically change its size." };

		var itemTemplate = new DataTemplate(() =>
		{
			var innerGrid = new Grid { Padding = new Thickness(10) };
			innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
			innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

			var image = new Image
			{
				Aspect = Aspect.AspectFill,
				HeightRequest = 60,
				WidthRequest = 60
			};
			image.SetBinding(Image.SourceProperty, new Binding("ImageUrl"));
			image.SetBinding(Image.AutomationIdProperty, new Binding("Name"));

			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += OnImageTapped;
			image.GestureRecognizers.Add(tapGesture);

			Grid.SetRowSpan(image, 2);

			var nameLabel = new Label { FontAttributes = FontAttributes.Bold };
			nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
			Grid.SetColumn(nameLabel, 1);

			var locationLabel = new Label
			{
				FontAttributes = FontAttributes.Italic,
				VerticalOptions = LayoutOptions.End
			};
			locationLabel.SetBinding(Label.TextProperty, new Binding("Location"));
			Grid.SetRow(locationLabel, 1);
			Grid.SetColumn(locationLabel, 1);

			innerGrid.Children.Add(image);
			innerGrid.Children.Add(nameLabel);
			innerGrid.Children.Add(locationLabel);

			return innerGrid;
		});

		var collectionView = new CollectionView
		{
			AutomationId = "Issue34783CollectionView",
			ItemTemplate = itemTemplate
		};
		collectionView.SetBinding(CollectionView.ItemsSourceProperty, new Binding("Monkeys"));

		Grid.SetRow(tapLabel, 1);
		Grid.SetRow(collectionView, 2);

		grid.Children.Add(instructions);
		grid.Children.Add(tapLabel);
		grid.Children.Add(collectionView);

		Content = grid;
	}

	void OnImageTapped(object sender, EventArgs e)
	{
		if (sender is Image image)
			image.HeightRequest = image.WidthRequest = image.HeightRequest.Equals(60) ? 100 : 60;
	}
}

public class Issue34783Monkey
{
	public string Name { get; set; }
	public string Location { get; set; }
	public string Details { get; set; }
	public string ImageUrl { get; set; }
}

public class Issue34783ViewModel
{
	public ObservableCollection<Issue34783Monkey> Monkeys { get; set; }

	public Issue34783ViewModel()
	{
		Monkeys = new ObservableCollection<Issue34783Monkey>
		{
			new Issue34783Monkey { Name = "Baboon",               Location = "Africa & Asia",               ImageUrl = "papio.jpg" },
			new Issue34783Monkey { Name = "Capuchin Monkey",      Location = "Central & South America",     ImageUrl = "capuchin.jpg" },
			new Issue34783Monkey { Name = "Blue Monkey",          Location = "Central and East Africa",     ImageUrl = "bluemonkey.jpg" },
			new Issue34783Monkey { Name = "Squirrel Monkey",      Location = "Central & South America",     ImageUrl = "saimiri.jpg" },
			new Issue34783Monkey { Name = "Golden Lion Tamarin",  Location = "Brazil",                      ImageUrl = "golden.jpg" },
			new Issue34783Monkey { Name = "Howler Monkey",        Location = "South America",               ImageUrl = "alouatta.jpg" },
			new Issue34783Monkey { Name = "Japanese Macaque",     Location = "Japan",                       ImageUrl = "papio.jpg" },
			new Issue34783Monkey { Name = "Mandrill",             Location = "Central Africa",              ImageUrl = "capuchin.jpg" },
			new Issue34783Monkey { Name = "Proboscis Monkey",     Location = "Borneo",                      ImageUrl = "bluemonkey.jpg" },
			new Issue34783Monkey { Name = "Red-shanked Douc",     Location = "Vietnam, Laos",               ImageUrl = "saimiri.jpg" },
			new Issue34783Monkey { Name = "Gray-shanked Douc",    Location = "Vietnam",                     ImageUrl = "golden.jpg" },
			new Issue34783Monkey { Name = "Snub-nosed Monkey",    Location = "China",                       ImageUrl = "alouatta.jpg" },
			new Issue34783Monkey { Name = "Black Snub-nosed",     Location = "China",                       ImageUrl = "papio.jpg" },
			new Issue34783Monkey { Name = "Tonkin Monkey",        Location = "Vietnam",                     ImageUrl = "capuchin.jpg" },
			new Issue34783Monkey { Name = "Thomas Langur",        Location = "Indonesia",                   ImageUrl = "bluemonkey.jpg" },
			new Issue34783Monkey { Name = "Purple-faced Langur",  Location = "Sri Lanka",                   ImageUrl = "saimiri.jpg" },
			new Issue34783Monkey { Name = "Gelada",               Location = "Ethiopia",                    ImageUrl = "golden.jpg" },
		};
	}
}
