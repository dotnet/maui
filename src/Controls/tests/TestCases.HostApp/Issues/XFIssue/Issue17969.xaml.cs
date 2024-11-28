using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 17969, "CollectionView duplicates group headers/footers when adding a new item to a group or crashes when adding a new group with empty view", PlatformAffected.iOS)]
	public partial class Issue17969 : ContentPage
	{
		GroupedCollectionViewModel viewModel;
		public Issue17969()
		{
			InitializeComponent();
			viewModel = new GroupedCollectionViewModel();
			BindingContext = viewModel;
		}

		private void OnAddClicked(object sender, EventArgs e)
		{
			viewModel.AddAnimal();
		}

		private void OnResetClicked(object sender, EventArgs e)
		{
			viewModel.Reset();
		}

	}

	public class Animal
	{
		public string Name { get; set; }
	}

	public class AnimalGroup : ObservableCollection<Animal>
	{
		public string Name { get; private set; }

		public AnimalGroup(string name, List<Animal> animals) : base(animals)
		{
			Name = name;
		}
	}

	public class GroupedCollectionViewModel
	{
		public GroupedCollectionViewModel()
		{
			Init();

		}

		private void Init()
		{
			AnimalBaseList.Clear();
			AnimalBaseList.Add(new AnimalGroup("Bears", new List<Animal>
			{
				new Animal
				{
					Name = "American Black Bear",
				},
				new Animal
				{
					Name = "Asian Black Bear",
				},
				new Animal
				{
					Name = "Brown Bear",
				},
				new Animal
				{
					Name = "Grizzly-Polar Bear Hybrid",
				},
				new Animal
				{
					Name = "Sloth Bear",
				},
				new Animal
				{
					Name = "Sun Bear",
				},
				new Animal
				{
					Name = "Polar Bear",
				},
				new Animal
				{
					Name = "Spectacled Bear",
				},
				new Animal
				{
					Name = "Short-faced Bear",
				},
				new Animal
				{
					Name = "California Grizzly Bear",
				}
			}));

			AnimalBaseList.Add(new AnimalGroup("Cats", new List<Animal>
			{
				new Animal
				{
					Name = "Abyssinian",
				},
				new Animal
				{
					Name = "Arabian Mau",
				},
				new Animal
				{
					Name = "Bengal",
				},
				new Animal
				{
					Name = "Burmese",
				},
				new Animal
				{
					Name = "Cyprus",
				},
				new Animal
				{
					Name = "German Rex",
				},
				new Animal
				{
					Name = "Highlander",
				},
				new Animal
				{
					Name = "Peterbald",
				},
				new Animal
				{
					Name = "Scottish Fold",
				},
				new Animal
				{
					Name = "Sphynx",
				}
			}));

			Animals.Add(new AnimalGroup(AnimalBaseList[0].Name, new List<Animal>()));

			AddAnimal();

			Animals.Add(new AnimalGroup(AnimalBaseList[1].Name, AnimalBaseList[1].ToList()));

		}


		public bool AddAnimal()
		{
			// Add animal from first group
			var sourceGroup = AnimalBaseList.First();
			var targetGroup = Animals.First();

			if (sourceGroup.Count == 0)
				return false;

			var animal = sourceGroup.First();
			targetGroup.Add(animal);
			sourceGroup.Remove(animal);

			return true;
		}

		public void Reset()
		{
			Animals.Clear();
			Init();

		}

		private List<AnimalGroup> AnimalBaseList { get; set; } = new List<AnimalGroup>();

		public ObservableCollection<AnimalGroup> Animals { get; private set; } = new ObservableCollection<AnimalGroup>();
	}

}