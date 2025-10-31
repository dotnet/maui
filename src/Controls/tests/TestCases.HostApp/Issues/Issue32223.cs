using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32223, "[Android] CollectionView items do not reorder correctly when using an item DataTemplateSelector", PlatformAffected.Android)]
public class Issue32223 : ContentPage
{
	public Issue32223()
	{
		var collectionView = new CollectionView
		{
			AutomationId = "ReorderableCollectionView",
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
			CanReorderItems = true
		};

		collectionView.BindingContext = new Issue32223ViewModel();

		var youngTemplate = new DataTemplate(() =>
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.LightGreen,
				Padding = 10,
				Margin = 5,
				RowDefinitions =
				{
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto }
				}
			};

			Grid.SetRowSpan(grid, 2);

			var nameLabel = new Label { FontSize = 18, TextColor = Colors.White };
			nameLabel.SetBinding(Label.TextProperty, "Name");

			var ageLabel = new Label { FontSize = 14, TextColor = Colors.DarkGreen };
			ageLabel.SetBinding(Label.TextProperty, new Binding("Age", stringFormat: "Age: {0}"));

			grid.Add(nameLabel, 0, 0);
			grid.Add(ageLabel, 0, 1);

			return grid;
		});

		// --- Adult Person Template ---
		var adultTemplate = new DataTemplate(() =>
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.LightBlue,
				Padding = 10,
				Margin = 5,
				RowDefinitions =
				{
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto }
				}
			};

			Grid.SetRowSpan(grid, 2);

			var nameLabel = new Label { FontSize = 18, TextColor = Colors.White };
			nameLabel.SetBinding(Label.TextProperty, "Name");

			var ageLabel = new Label { FontSize = 14, TextColor = Colors.DarkBlue };
			ageLabel.SetBinding(Label.TextProperty, new Binding("Age", stringFormat: "Age: {0}"));

			grid.Add(nameLabel, 0, 0);
			grid.Add(ageLabel, 0, 1);

			return grid;
		});

		// --- Template Selector ---
		var selector = new Issue32223TemplateSelector
		{
			YoungTemplate = youngTemplate,
			AdultTemplate = adultTemplate
		};

		// --- CollectionView Setup ---
		collectionView.SetBinding(ItemsView.ItemsSourceProperty, "People");

		collectionView.ItemTemplate = selector;

		Content = new Grid
		{
			Children = { collectionView }
		};

	}
}

public class Issue32223Model
{
	public string Name { get; set; }
	public int Age { get; set; }
}

public class Issue32223TemplateSelector : DataTemplateSelector
{
	public DataTemplate YoungTemplate { get; set; }
	public DataTemplate AdultTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		if (item is Issue32223Model person)
			return person.Age < 30 ? YoungTemplate : AdultTemplate;

		return null;
	}
}

public class Issue32223ViewModel
{
	public ObservableCollection<Issue32223Model> People { get; set; }

	public Issue32223ViewModel()
	{
		People = new ObservableCollection<Issue32223Model>
		{
			new Issue32223Model { Name = "Alice", Age = 25 },
			new Issue32223Model { Name = "Bob", Age = 35 },
			new Issue32223Model { Name = "Charlie", Age = 28 },
			new Issue32223Model { Name = "David", Age = 40 }
		};
	}
}