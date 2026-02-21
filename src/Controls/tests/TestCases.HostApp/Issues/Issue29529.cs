using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29529, "CurrentItemChangedEventArgs and PositionChangedEventArgs Not Updating Correctly in CarouselView", PlatformAffected.UWP | PlatformAffected.Android)]
public class Issue29529 : ContentPage
{
	int _positionChangedCount = 0;
	int _currentItemChangedCount = 0;

	public Issue29529()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
			"Item 5",
			"Item 6",
		};

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			Position = 3,
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView,
			Loop = false,
			HeightRequest = 300,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Padding = 10
				};

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 18,
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				grid.Children.Add(label);
				return grid;
			}),
			HorizontalOptions = LayoutOptions.Fill,
		};

		var positionLabel = new Label
		{
			AutomationId = "positionLabel",
			Text = $"Current Position{carouselView.Position}",
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(20),
		};

		var itemLabel = new Label
		{
			AutomationId = "itemLabel",
			Text = $"Current Item{carouselView.CurrentItem}",
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(20),
		};

		var eventCountLabel = new Label
		{
			AutomationId = "eventCountLabel",
			Text = "PositionChanged: 0, CurrentItemChanged: 0",
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(20),
		};

		carouselView.PositionChanged += (s, e) =>
		{
			_positionChangedCount++;
			positionLabel.Text = $"Current Position: {e.CurrentPosition}, Previous Position: {e.PreviousPosition}";
			eventCountLabel.Text = $"PositionChanged: {_positionChangedCount}, CurrentItemChanged: {_currentItemChangedCount}";
		};

		carouselView.CurrentItemChanged += (s, e) =>
		{
			_currentItemChangedCount++;
			itemLabel.Text = $"Current Item: {e.CurrentItem}, Previous Item: {e.PreviousItem}";
			eventCountLabel.Text = $"PositionChanged: {_positionChangedCount}, CurrentItemChanged: {_currentItemChangedCount}";
		};

		var insertButton = new Button
		{
			Text = "Insert item at index 6",
			AutomationId = "InsertButton",
			Margin = new Thickness(20),
		};

		insertButton.Clicked += (sender, e) =>
		{
			_positionChangedCount = 0;
			_currentItemChangedCount = 0;
			carouselItems.Insert(6, "Item 0");
		};

		verticalStackLayout.Children.Add(carouselView);
		verticalStackLayout.Children.Add(insertButton);
		verticalStackLayout.Children.Add(positionLabel);
		verticalStackLayout.Children.Add(itemLabel);
		verticalStackLayout.Children.Add(eventCountLabel);
		Content = verticalStackLayout;
	}
}
