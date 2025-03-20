namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 23293, "'Grouping for Vertical list without DataTemplates' page loading exception", PlatformAffected.UWP)]
public class Issue23293 : ContentPage
{
	public List<AnimalGroup> Animals { get; private set; } = new List<AnimalGroup>();

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
		Animals.Add(new AnimalGroup("Bears", new List<Animal>
		{
			new Animal { Name = "American Black Bear" },
			new Animal { Name = "Asian Black Bear" },
			new Animal { Name = "Brown Bear" },
			new Animal { Name = "Grizzly-Polar Bear Hybrid" },
			new Animal { Name = "Sloth Bear" },
		}));

		Animals.Add(new AnimalGroup("Cats", new List<Animal>
		{
			new Animal { Name = "Abyssinian" },
			new Animal { Name = "Arabian Mau" },
			new Animal { Name = "Bengal" },
			new Animal { Name = "Burmese" },
			new Animal { Name = "Cyprus" },
		}));

		Animals.Add(new AnimalGroup("Dogs", new List<Animal>
		{
			new Animal { Name = "Afghan Hound" },
			new Animal { Name = "Alpine Dachsbracke" },
			new Animal { Name = "American Bulldog" },
			new Animal { Name = "Bearded Collie" },
			new Animal { Name = "Boston Terrier" },
		}));

		Animals.Add(new AnimalGroup("Elephants", new List<Animal>
		{
			new Animal { Name = "African Bush Elephant" },
			new Animal { Name = "African Forest Elephant" },
			new Animal { Name = "Desert Elephant" },
			new Animal { Name = "Borneo Elephant" },
			new Animal { Name = "Indian Elephant" },
		}));

		Animals.Add(new AnimalGroup("Monkeys", new List<Animal>
		{
			new Animal { Name = "Baboon" },
			new Animal { Name = "Capuchin Monkey" },
			new Animal { Name = "Blue Monkey" },
			new Animal { Name = "Squirrel Monkey" },
			new Animal { Name = "Golden Lion Tamarin" },
		}));
	}
}

public class AnimalGroup : List<Animal>
{
	public string Name { get; }

	public AnimalGroup(string name, List<Animal> animals) : base(animals)
	{
		Name = name;
	}
}

public class Animal
{
	public string Name { get; set; }

	public override string ToString()
	{
		return Name;  
	}
}
