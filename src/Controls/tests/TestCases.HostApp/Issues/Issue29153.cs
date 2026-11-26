using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29153, "Grouped CollectionView keeps the last data item in view", PlatformAffected.Android)]
public class Issue29153 : ContentPage
{
	public Issue29153()
	{
		var first = new ObservableCollection<string>();
		var last = new ObservableCollection<string>();
		for (int i = 0; i < 30; i++)
		{
			first.Add($"First {i}");
			last.Add($"Last {i}");
		}

		var groups = new ObservableCollection<ObservableCollection<string>>
		{
			first, last, new ObservableCollection<string>()
		};
		var collectionView = new CollectionView
		{
			AutomationId = "GroupedCollection",
			IsGrouped = true,
			ItemsSource = groups,
			Header = "Collection header",
			Footer = "Collection footer",
			GroupHeaderTemplate = new DataTemplate(() => new Label { Text = "Group header", HeightRequest = 40 }),
			GroupFooterTemplate = new DataTemplate(() => new Label { Text = "Group footer", HeightRequest = 40 }),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { HeightRequest = 60 };
				label.SetBinding(Label.TextProperty, static (string item) => item);
				label.SetBinding(AutomationIdProperty, static (string item) => item);
				return label;
			})
		};

		var add = new Button { Text = "Append to last non-empty group", AutomationId = "AppendItem" };
		add.Clicked += (_, _) =>
		{
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
			last.Add($"Last {last.Count}");
		};

		var clear = new Button { Text = "Clear groups", AutomationId = "ClearGroups" };
		clear.Clicked += (_, _) =>
		{
			first.Clear();
			last.Clear();
			groups.Clear();
		};

		var layout = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};
		layout.Add(add, 0, 0);
		layout.Add(clear, 0, 1);
		layout.Add(collectionView, 0, 2);
		Content = layout;
	}
}
