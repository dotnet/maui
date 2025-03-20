namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 23293, "'Grouping for Vertical list without DataTemplates' page loading exception", PlatformAffected.UWP)]
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
		Animals.Add(new Issue23293AnimalGroup("Bears", new List<Issue23293Animal>
		{
			new Issue23293Animal { Name = "American Black Bear" },
			new Issue23293Animal { Name = "Asian Black Bear" },
			new Issue23293Animal { Name = "Brown Bear" },
			new Issue23293Animal { Name = "Grizzly-Polar Bear Hybrid" },
			new Issue23293Animal { Name = "Sloth Bear" },
		}));

		Animals.Add(new Issue23293AnimalGroup("Cats", new List<Issue23293Animal>
		{
			new Issue23293Animal { Name = "Abyssinian" },
			new Issue23293Animal { Name = "Arabian Mau" },
			new Issue23293Animal { Name = "Bengal" },
			new Issue23293Animal { Name = "Burmese" },
			new Issue23293Animal { Name = "Cyprus" },
		}));

		Animals.Add(new Issue23293AnimalGroup("Dogs", new List<Issue23293Animal>
		{
			new Issue23293Animal { Name = "Afghan Hound" },
			new Issue23293Animal { Name = "Alpine Dachsbracke" },
			new Issue23293Animal { Name = "American Bulldog" },
			new Issue23293Animal { Name = "Bearded Collie" },
			new Issue23293Animal { Name = "Boston Terrier" },
		}));

		Animals.Add(new Issue23293AnimalGroup("Elephants", new List<Issue23293Animal>
		{
			new Issue23293Animal { Name = "African Bush Elephant" },
			new Issue23293Animal { Name = "African Forest Elephant" },
			new Issue23293Animal { Name = "Desert Elephant" },
			new Issue23293Animal { Name = "Borneo Elephant" },
			new Issue23293Animal { Name = "Indian Elephant" },
		}));

		Animals.Add(new Issue23293AnimalGroup("Monkeys", new List<Issue23293Animal>
		{
			new Issue23293Animal { Name = "Baboon" },
			new Issue23293Animal { Name = "Capuchin Monkey" },
			new Issue23293Animal { Name = "Blue Monkey" },
			new Issue23293Animal { Name = "Squirrel Monkey" },
			new Issue23293Animal { Name = "Golden Lion Tamarin" },
		}));
	}
}

public class Issue23293AnimalGroup : List<Issue23293Animal>
{
	public string Name { get; }

	public Issue23293AnimalGroup(string name, List<Issue23293Animal> animals) : base(animals)
	{
		Name = name;
	}
}

public class Issue23293Animal
{
	public string Name { get; set; }

	public override string ToString()
	{
		return Name;  
	}
}
