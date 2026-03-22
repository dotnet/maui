using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31535, "[iOS] Crash occurred on CarouselView2 when deleting last one remaining item with loop as false", PlatformAffected.iOS)]
public class Issue31535 : ContentPage
{
	ObservableCollection<string> _items = new();
	public ObservableCollection<string> Items
	{
		get => _items;
		set
		{
			_items = value;
			OnPropertyChanged();
		}
	}
	public Issue31535()
	{
		// Start with empty collection to test all scenarios
		Items = new ObservableCollection<string>();

		// Create CarouselView
		var carousel = new CarouselView2
		{
			Loop = false,
			HeightRequest = 200,
			ItemsSource = Items,
			AutomationId = "TestCarouselView",
			ItemTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					Margin = 10,
					WidthRequest = 200,
					BackgroundColor = Colors.Red
				};

				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");

				border.Content = label;
				return border;
			})
		};

		// Create Buttons for testing different scenarios
		var addSingleItemButton = new Button
		{
			AutomationId = "AddSingleItemButton",
			Text = "Add Single Item"
		};

		var removeSingleItemButton = new Button
		{
			AutomationId = "RemoveSingleItemButton",
			Text = "Remove Single Item"
		};

		var addMultipleItemsButton = new Button
		{
			AutomationId = "AddMultipleItemsButton",
			Text = "Add Multiple Items"
		};

		var removeAllItemsButton = new Button
		{
			AutomationId = "RemoveAllItemsButton",
			Text = "Remove All Items"
		};

		var itemCountLabel = new Label
		{
			Text = $"Items Count: {Items.Count}",
			HorizontalOptions = LayoutOptions.Center
		};

		// Event handlers
		addSingleItemButton.Clicked += (s, e) =>
		{
			Items.Add($"Item {Items.Count + 1}");
			itemCountLabel.Text = $"Items Count: {Items.Count}";
		};

		removeSingleItemButton.Clicked += (s, e) =>
		{
			if (Items.Count > 0)
			{
				Items.RemoveAt(Items.Count - 1);
				itemCountLabel.Text = $"Items Count: {Items.Count}";
			}
		};

		addMultipleItemsButton.Clicked += (s, e) =>
		{
			var currentCount = Items.Count;
			Items.Add($"Item {currentCount + 1}");
			Items.Add($"Item {currentCount + 2}");
			Items.Add($"Item {currentCount + 3}");
			itemCountLabel.Text = $"Items Count: {Items.Count}";
		};

		removeAllItemsButton.Clicked += (s, e) =>
		{
			Items.Clear();
			itemCountLabel.Text = $"Items Count: {Items.Count}";
		};

		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "CarouselView2 Loop=false Test",
					FontSize = 18,
					HorizontalOptions = LayoutOptions.Center
				},
				itemCountLabel,
				carousel,
				addSingleItemButton,
				removeSingleItemButton,
				addMultipleItemsButton,
				removeAllItemsButton
			}
		};
		BindingContext = this;
	}
}