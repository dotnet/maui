using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33130, "CollectionView group header size changes with ItemSizingStrategy", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue33130 : ContentPage
{
	public Issue33130()
	{
		InitializeComponent();
		BindingContext = new GroupedAnimalsViewModel();
	}

	private void OnSwitchToMeasureFirstItem(object sender, EventArgs e)
	{
		TestCollectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
		StatusLabel.Text = $"ItemSizingStrategy: {TestCollectionView.ItemSizingStrategy}";
	}
}

public class GroupedAnimalsViewModel
{
	public ObservableCollection<GroupHeaderTestAnimalGroup> Animals { get; set; }

	public GroupedAnimalsViewModel()
	{
		Animals = new ObservableCollection<GroupHeaderTestAnimalGroup>
		{
			new GroupHeaderTestAnimalGroup("Bears")
			{
				new GroupHeaderTestAnimal { Name = "Grizzly Bear", Location = "North America", ImageUrl = "bear.jpg" },
				new GroupHeaderTestAnimal { Name = "Polar Bear", Location = "Arctic", ImageUrl = "bear.jpg" },
			},
			new GroupHeaderTestAnimalGroup("Monkeys")
			{
				new GroupHeaderTestAnimal { Name = "Baboon", Location = "Africa", ImageUrl = "monkey.jpg" },
				new GroupHeaderTestAnimal { Name = "Capuchin Monkey", Location = "South America", ImageUrl = "monkey.jpg" },
				new GroupHeaderTestAnimal { Name = "Spider Monkey", Location = "Central America", ImageUrl = "monkey.jpg" },
			},
			new GroupHeaderTestAnimalGroup("Elephants")
			{
				new GroupHeaderTestAnimal { Name = "African Elephant", Location = "Africa", ImageUrl = "elephant.jpg" },
				new GroupHeaderTestAnimal { Name = "Asian Elephant", Location = "Asia", ImageUrl = "elephant.jpg" },
			}
		};
	}
}

public class GroupHeaderTestAnimalGroup : ObservableCollection<GroupHeaderTestAnimal>
{
	public string Name { get; set; }

	public GroupHeaderTestAnimalGroup(string name) : base()
	{
		Name = name;
	}
}

public class GroupHeaderTestAnimal
{
	public string Name { get; set; }
	public string Location { get; set; }
	public string ImageUrl { get; set; }
}
