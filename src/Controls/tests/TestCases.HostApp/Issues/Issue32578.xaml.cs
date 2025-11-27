using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32578, "Grouped CollectionView doesn't size correctly when ItemSizingStrategy=MeasureFirstItem", PlatformAffected.Android)]
	public partial class Issue32578 : ContentPage
	{
		public ObservableCollection<AnimalGroup32578> Animals { get; set; } = new();

		public Issue32578()
		{
			InitializeComponent();

			// Populate data similar to the reproduction project
			Animals.Add(new AnimalGroup32578("Bears", new List<Animal32578>
			{
				new Animal32578
				{
					Name = "American Black Bear",
					Location = "North America"
				},
				new Animal32578
				{
					Name = "Asian Black Bear",
					Location = "Asia"
				}
			}));

			Animals.Add(new AnimalGroup32578("Monkeys", new List<Animal32578>
			{
				new Animal32578
				{
					Name = "Baboon",
					Location = "Africa & Asia"
				},
				new Animal32578
				{
					Name = "Capuchin Monkey",
					Location = "Central & South America"
				},
				new Animal32578
				{
					Name = "Blue Monkey",
					Location = "Central and East Africa"
				}
			}));

			BindingContext = this;
		}
	}

	public class Animal32578
	{
		public string Name { get; set; }
		public string Location { get; set; }
	}

	public class AnimalGroup32578 : List<Animal32578>
	{
		public string Name { get; private set; }

		public AnimalGroup32578(string name, List<Animal32578> animals) : base(animals)
		{
			Name = name;
		}
	}
}
