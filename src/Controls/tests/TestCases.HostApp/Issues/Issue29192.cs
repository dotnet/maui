using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29192, "[Android] CollectionView MeasureFirstItem ItemSizingStrategy Not Applied in Horizontal Layouts", PlatformAffected.Android)]
public class Issue29192 : ContentPage
{
	public Issue29192()
	{
		CollectionView collectionView = new CollectionView();

		var layout = new Grid();
		var bindingContext = new Issue29192ViewModel();

		collectionView.BindingContext = bindingContext;
		collectionView.AutomationId = "CollectionView";
		collectionView.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
		collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, "Item");
			return label;
		});

		// Set the ItemsSource binding
		collectionView.SetBinding(CollectionView.ItemsSourceProperty, "Items");
		layout.Children.Add(collectionView);

		Content = layout;
	}
}


public class Issue29192ViewModel
{
	public ObservableCollection<Issue29192Modal> Items { get; set; }

	public Issue29192ViewModel()
	{
		Items = new ObservableCollection<Issue29192Modal>
		{
			new Issue29192Modal { Item = "Short text." },
			new Issue29192Modal { Item = "This is a long text; it should be wrapped to avoid truncation or overflow." },
			new Issue29192Modal { Item = "This is very long text." }
		};
	}
}

public class Issue29192Modal
{
	public string Item { get; set; }
}