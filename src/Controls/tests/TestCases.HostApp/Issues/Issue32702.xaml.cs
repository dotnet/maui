using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32702, "CollectionView item selection doesn't work when DragGestureRecognizer or DropGestureRecognizer is attached to item content", PlatformAffected.Android)]
public partial class Issue32702 : ContentPage
{
	public ObservableCollection<string> Items { get; set; }

	public Issue32702()
	{
		InitializeComponent();

		Items = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"
			};

		TestCollectionView.ItemsSource = Items;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.Count > 0)
		{
			var selectedItem = e.CurrentSelection[0].ToString();
			StatusLabel.Text = $"Selected: {selectedItem}";
		}
		else
		{
			StatusLabel.Text = "No selection";
		}
	}
}
