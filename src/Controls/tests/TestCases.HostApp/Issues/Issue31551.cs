namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31551, "ArgumentOutOfRangeException thrown by ScrollTo when group index is invalid", PlatformAffected.Android)]
public class Issue31551 : ContentPage
{
	CollectionView _collectionView;
	public List<Issue31551AnimalGroup> Animals { get; set; } = new List<Issue31551AnimalGroup>();
	const int VALID_ITEM_INDEX = 1;
	const int INVALID_GROUP_INDEX = 10;
	public Issue31551()
	{
		CreateAnimalsCollection();

		Button scrollButton = new Button
		{
			AutomationId = "Issue31551ScrollBtn",
			Text = "Scroll to Invalid Group"
		};
		scrollButton.Clicked += ScrollButton_Clicked;

		_collectionView = new CollectionView
		{
			AutomationId = "Issue31551CollectionView",
			IsGrouped = true,
			ItemsSource = Animals,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontAttributes = FontAttributes.Bold,
					BackgroundColor = Colors.LightGray,
				};

				label.SetBinding(Label.TextProperty, "Name");
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				Label textLabel = new Label
				{
					FontSize = 20,
					HeightRequest = 40,
					FontAttributes = FontAttributes.Bold,
				};

				textLabel.SetBinding(Label.TextProperty, ".");
				return textLabel;
			})
		};

		Grid grid = new Grid
		{
			RowSpacing = 10,
			Padding = 10,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		grid.Add(scrollButton, 0, 0);
		grid.Add(_collectionView, 0, 1);

		Content = grid;
	}

	void CreateAnimalsCollection()
	{
		Animals.Add(new Issue31551AnimalGroup("Bears", new List<string>
		{
			"American Black Bear",
			"Asian Black Bear",
			"Brown Bear",
			"Polar Bear"
		}));

		Animals.Add(new Issue31551AnimalGroup("Cats", new List<string>
		{
			"American Shorthair",
			"Bengal",
			"Sphynx",
			"Persian"
		}));

		Animals.Add(new Issue31551AnimalGroup("Dogs", new List<string>
		{
			"German Shepherd",
			"Golden Retriever",
			"Bulldog",
			"Poodle"
		}));
	}

	private void ScrollButton_Clicked(object sender, EventArgs e)
	{
		_collectionView.ScrollTo(VALID_ITEM_INDEX, INVALID_GROUP_INDEX, ScrollToPosition.Start, true);
	}
}

public class Issue31551AnimalGroup :  List<string>
{
	public string Name { get; set; }
	
	public Issue31551AnimalGroup(string name, IEnumerable<string> items) : base(items)
	{
		Name = name;
	}
}