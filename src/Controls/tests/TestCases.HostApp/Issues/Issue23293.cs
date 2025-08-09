namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 23293, "Grouping collection view without data template results in displaying the default string representation of the object", PlatformAffected.UWP)]
public class Issue23293 : ContentPage
{
	public List<Issue23293AnimalGroup> Animals { get; set; } = new List<Issue23293AnimalGroup>();

	public Issue23293()
	{
		CreateAnimalsCollection();

		var collectionViewWithoutDataTemplate = new CollectionView
		{
			AutomationId = "CollectionViewWithoutDataTemplate",
			ItemsSource = Animals,
			IsGrouped = true,
		};

		var grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

		grid.Add(collectionViewWithoutDataTemplate, 0, 0);

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