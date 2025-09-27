using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29524, "Previous Position in PositionChangedEventArgs Not Updating Correctly in CarouselView", PlatformAffected.iOS)]
public class Issue29524 : ContentPage
{
	public Issue29524()
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

		CarouselView2 carouselView = new CarouselView2
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView,
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

		carouselView.PositionChanged += (s, e) =>
		{
			positionLabel.Text = $"Current Position: {e.CurrentPosition}, Previous Position: {e.PreviousPosition}";
		};

		var insertButton = new Button
		{
			Text = "Insert item at index 0",
			AutomationId = "InsertButton",
			Margin = new Thickness(20),
		};

		insertButton.Clicked += (sender, e) =>
		{
			carouselItems.Insert(0, "Item 0");
		};

		var removeButton = new Button
		{
			Text = "Remove an item",
			AutomationId = "RemoveButton",
			Margin = new Thickness(20),
		};

		removeButton.Clicked += (sender, e) =>
		{
			carouselView.Position = 2;
			carouselItems.RemoveAt(2);
		};

		verticalStackLayout.Children.Add(carouselView);
		verticalStackLayout.Children.Add(insertButton);
		verticalStackLayout.Children.Add(removeButton);
		verticalStackLayout.Children.Add(positionLabel);
		Content = verticalStackLayout;
	}
}