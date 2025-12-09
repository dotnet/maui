using System.Collections;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22320, "CollectionView does not highlight selected grouped items correctly", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue22320 : ContentPage
{
	public Issue22320()
	{
		var layout = new StackLayout();
		var bindingContext = new Issue22320ViewModel();
		var collectionView = new CollectionView();
		collectionView.BindingContext = bindingContext;
		collectionView.IsGrouped = true;
		collectionView.AutomationId = "CollectionView";
		collectionView.SelectionMode = SelectionMode.Single;

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var border = new Border
			{
				HeightRequest = 20
			};

			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");

			border.Content = label;

			return border;
		});

		collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var border = new Border
			{
				Background = Colors.LightGrey,
				HeightRequest = 20
			};

			var label = new Label();
			label.SetBinding(Label.TextProperty, "Key");

			border.Content = label;

			return border;
		});

		// Set the ItemsSource binding
		collectionView.SetBinding(CollectionView.ItemsSourceProperty, "Items");
		layout.Children.Add(collectionView);

		Content = layout;
	}
}

public class Issue22320ItemGroup : IEnumerable<string>
{
	private readonly string _key;
	private readonly IEnumerable<string> _items;

	public Issue22320ItemGroup(string key, IEnumerable<string> items)
	{
		_key = key;
		_items = items;
	}

	public string Key => _key;

	public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Issue22320ViewModel
{
	public Issue22320ViewModel()
	{

		Items.Add(new Issue22320ItemGroup("Vegetables", new[] { "Carrot", "Broccoli", "Spinach", "Potato", "Tomato" }));
		Items.Add(new Issue22320ItemGroup("Fruits", new[] { "Apple", "Banana", "Orange", "Grapes", "Mango" }));
	}

	public ObservableCollection<Issue22320ItemGroup> Items { get; } = new();
}


