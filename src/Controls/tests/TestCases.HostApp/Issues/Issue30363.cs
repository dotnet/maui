using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30363, "[iOS] CollectionView does not clear selection when SelectedItem is set to null", PlatformAffected.iOS)]
public class Issue30363 : ContentPage
{
	public Issue30363()
	{
		ObservableCollection<string> Items = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
			"Item 5"
		};


		var collectionView = new CollectionView
		{
			ItemsSource = Items,
			SelectionMode = SelectionMode.Single,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 24,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.Center,
					AutomationId = "cvItem"
				};
				label.SetBinding(Label.TextProperty, ".");

				return new Grid
				{
					Children = { label }
				};
			})
		};

		collectionView.SelectionChanged += (sender, e) =>
			{
				if (e.CurrentSelection.Count > 0)
				{
					var selectedItem = e.CurrentSelection[0] as string;
					if (selectedItem != null)
					{
						// Deselect the item
						collectionView.SelectedItem = null;
					}
				}
			};

		var grid = new Grid();
		grid.Children.Add(collectionView);

		Content = grid;
	}
}