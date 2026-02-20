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
		BindingContext = new Issue33130ViewModel();
	}

	private void OnSwitchToMeasureFirstItem(object sender, EventArgs e)
	{
		TestCollectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
		StatusLabel.Text = $"ItemSizingStrategy: {TestCollectionView.ItemSizingStrategy}";
	}
}

public class Issue33130ViewModel
{
	public ObservableCollection<Issue33130AnimalGroup> Animals { get; set; }

	public Issue33130ViewModel()
	{
		Animals = new ObservableCollection<Issue33130AnimalGroup>
		{
			new Issue33130AnimalGroup("Bears")
			{
				new Issue33130Animal { Name = "Grizzly Bear", Location = "North America", ImageUrl = "bear.jpg" },
				new Issue33130Animal { Name = "Polar Bear", Location = "Arctic", ImageUrl = "bear.jpg" },
			},
			new Issue33130AnimalGroup("Monkeys")
			{
				new Issue33130Animal { Name = "Baboon", Location = "Africa", ImageUrl = "monkey.jpg" },
				new Issue33130Animal { Name = "Capuchin Monkey", Location = "South America", ImageUrl = "monkey.jpg" },
				new Issue33130Animal { Name = "Spider Monkey", Location = "Central America", ImageUrl = "monkey.jpg" },
			},
			new Issue33130AnimalGroup("Elephants")
			{
				new Issue33130Animal { Name = "African Elephant", Location = "Africa", ImageUrl = "elephant.jpg" },
				new Issue33130Animal { Name = "Asian Elephant", Location = "Asia", ImageUrl = "elephant.jpg" },
			}
		};
	}
}

public class Issue33130AnimalGroup : ObservableCollection<Issue33130Animal>
{
	public string Name { get; set; }

	public Issue33130AnimalGroup(string name) : base()
	{
		Name = name;
	}
}

public class Issue33130Animal
{
	public string Name { get; set; }
	public string Location { get; set; }
	public string ImageUrl { get; set; }
}
