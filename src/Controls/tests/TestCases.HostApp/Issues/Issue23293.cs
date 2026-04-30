namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 23293, "Grouping collection view without data template results in displaying the default string representation of the object", PlatformAffected.UWP)]
public class Issue23293 : ContentPage
{
	public List<Issue23293AnimalGroup> Animals { get; set; } = new List<Issue23293AnimalGroup>();

	public Issue23293()
	{
		CreateAnimalsCollection();

		CollectionView collectionViewWithoutDataTemplate = new CollectionView
		{
			AutomationId = "CollectionViewWithoutDataTemplate",
			ItemsSource = Animals,
			IsGrouped = true,
		};

		CollectionView collectionViewWithGroupTemplates = new CollectionView
		{
			AutomationId = "CollectionViewWithGroupTemplates",
			ItemsSource = Animals,
			IsGrouped = true,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				Label label = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, BackgroundColor = Colors.LightGray };
				label.SetBinding(Label.TextProperty, "Name");
				return label;
			}),
			GroupFooterTemplate = new DataTemplate(() =>
			{
				Label label = new Label { FontSize = 12, BackgroundColor = Colors.LightBlue, AutomationId = "GroupFooterLabel" };
				label.SetBinding(Label.TextProperty, "Name", stringFormat: "End of {0}");
				return label;
			}),
		};

		Grid grid = new Grid
		{
			Padding = 10,
		};
		
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

		grid.Add(new Label { Text = "No ItemTemplate - items should show animal names", FontSize = 12, FontAttributes = FontAttributes.Bold }, 0, 0);
		grid.Add(collectionViewWithoutDataTemplate, 0, 1);
		grid.Add(new Label { Text = "No ItemTemplate with Header/Footer - header shows group name, items show animal name, footer shows 'End of Bears'", FontSize = 12, FontAttributes = FontAttributes.Bold }, 0, 2);
		grid.Add(collectionViewWithGroupTemplates, 0, 3);

		Content = grid;
	}

	void CreateAnimalsCollection()
	{
		Animals.Add(new Issue23293AnimalGroup("Bears", new List<string>
		{
			"American Black Bear",
		}));
	}
}

public class Issue23293AnimalGroup : List<string>
{
	public string Name { get; set; }

	public Issue23293AnimalGroup(string name, List<string> animals) : base(animals)
	{
		Name = name;
	}
}
