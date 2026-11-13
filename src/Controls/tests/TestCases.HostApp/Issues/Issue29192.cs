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
			label.SetBinding(Label.TextProperty, ".");
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
	public ObservableCollection<string> Items { get; set; }

	public Issue29192ViewModel()
	{
		Items = new ObservableCollection<string>
		{
			"Short text.",
			"This is a long text; it should be wrapped to avoid truncation or overflow.",
			"This is very long text."
		};
	}
}
