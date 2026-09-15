using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37361, "RefreshView pull-to-refresh does nothing when CollectionView is empty", PlatformAffected.iOS)]
public class Issue37361 : ContentPage
{
	readonly ObservableCollection<string> _items = [];
	readonly Label _statusLabel;
	int _refreshCount;

	public Issue37361()
	{
		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Refreshes: 0"
		};

		var collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			ItemsSource = _items,
			EmptyView = new Label
			{
				AutomationId = "EmptyViewLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "No items"
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		var refreshView = new RefreshView
		{
			AutomationId = "RefreshView",
			Content = collectionView
		};
		refreshView.Command = new Command(() =>
		{
			_refreshCount++;
			_statusLabel.Text = $"Refreshes: {_refreshCount}";
			refreshView.IsRefreshing = false;
		});

		var addButton = new Button
		{
			AutomationId = "AddItemButton",
			Text = "Add item",
			Command = new Command(() => _items.Add("Item"))
		};

		var clearButton = new Button
		{
			AutomationId = "ClearItemsButton",
			Text = "Clear items",
			Command = new Command(_items.Clear)
		};

		var buttonLayout = new HorizontalStackLayout
		{
			Children = { addButton, clearButton }
		};
		Grid.SetRow(buttonLayout, 1);

		var refreshBorder = new Border
		{
			Content = refreshView
		};
		Grid.SetRow(refreshBorder, 2);

		Content = new Grid
		{
			Padding = 12,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				_statusLabel,
				buttonLayout,
				refreshBorder
			}
		};
	}
}