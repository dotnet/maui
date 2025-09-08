namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14141, "ArgumentOutOfRangeException thrown by ScrollTo when group index is invalid", PlatformAffected.Android)]
public class Issue14141 : ContentPage
{
	CollectionView _collectionView;
	Label descriptionLabel;
	public List<Issue14141AnimalGroup> Animals { get; set; } = new List<Issue14141AnimalGroup>();
	public Issue14141()
	{
		CreateAnimalsCollection();

		Button scrollButton = new Button
		{
			AutomationId = "Issue14141ScrollBtn",
			Text = "Scroll"
		};
		scrollButton.Clicked += ScrollButton_Clicked;

		descriptionLabel = new Label
		{
			AutomationId = "Issue14141StatusLabel",
			Text = "Status"
		};

		_collectionView = new CollectionView
		{
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
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		grid.Add(scrollButton, 0, 0);
		grid.Add(descriptionLabel, 0, 1);
		grid.Add(_collectionView, 0, 2);

		Content = grid;
	}

	void CreateAnimalsCollection()
	{
		Animals.Add(new Issue14141AnimalGroup("Bears", new List<string>
		{
			"American Black Bear",
			"Asian Black Bear",
			"Brown Bear",
			"Polar Bear"
		}));

		Animals.Add(new Issue14141AnimalGroup("Cats", new List<string>
		{
			"American Shorthair",
			"Bengal",
			"Sphynx",
			"Persian"
		}));

		Animals.Add(new Issue14141AnimalGroup("Dogs", new List<string>
		{
			"German Shepherd",
			"Golden Retriever",
			"Bulldog",
			"Poodle"
		}));
	}

	private void ScrollButton_Clicked(object sender, EventArgs e)
	{
		try
		{
			_collectionView.ScrollTo(1, -3, ScrollToPosition.Start, true);
			descriptionLabel.Text = "Success";
		}
		catch
		{
			descriptionLabel.Text = "Failure";
		}
	}
}

public class Issue14141AnimalGroup :  List<string>
{
	public string Name { get; set; }
	
	public Issue14141AnimalGroup(string name, IEnumerable<string> items) : base(items)
	{
		Name = name;
	}
}