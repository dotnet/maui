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
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

		grid.Add(collectionViewWithoutDataTemplate, 0, 1);
		
		Content = grid;
	}

	void CreateAnimalsCollection()
	{
		Animals.Add(new Issue23293AnimalGroup("Bears", new List<string>
		{
			"American Black Bear",
			"Asian Black Bear",
			"Brown Bear",
			"Grizzly-Polar Bear Hybrid",
			"Sloth Bear",
		}));

		Animals.Add(new Issue23293AnimalGroup("Cats", new List<string>
		{
			"Abyssinian",
			"Arabian Mau",
			"Bengal",
			"Burmese",
			"Cyprus",
		}));

		Animals.Add(new Issue23293AnimalGroup("Dogs", new List<string>
		{
			"Afghan Hound",
			"Alpine Dachsbracke",
			"American Bulldog",
			"Bearded Collie",
			"Boston Terrier",
		}));

		Animals.Add(new Issue23293AnimalGroup("Elephants", new List<string>
		{
			"African Bush Elephant",
			"African Forest Elephant",
			"Desert Elephant",
			"Borneo Elephant",
			"Indian Elephant",
		}));

		Animals.Add(new Issue23293AnimalGroup("Monkeys", new List<string>
		{
			"Baboon",
			"Capuchin Monkey",
			"Blue Monkey",
			"Squirrel Monkey",
			"Golden Lion Tamarin",
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