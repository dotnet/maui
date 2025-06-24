using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29372, "CarouselView ItemsLayout Not Updating at Runtime", PlatformAffected.All)]
public partial class Issue29372 : ContentPage
{
	public Issue29372()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"Item 0",
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
		};

		CarouselView2 carouselView = new CarouselView2
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			PeekAreaInsets = new Thickness(0, 100),
			HeightRequest = 300,
			Loop = false,
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

		var button = new Button
		{
			Text = "Change Items Layout",
			AutomationId = "ChangeItemsLayoutButton",
			Margin = new Thickness(20),
		};

		var label = new Label
		{
			Text = "The test is passed if the items are displayed in a vertical orientation.",
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(20),
		};

		button.Clicked += (sender, e) =>
		{
			carouselView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
		};

		verticalStackLayout.Children.Add(label);
		verticalStackLayout.Children.Add(carouselView);
		verticalStackLayout.Children.Add(button);
		Content = verticalStackLayout;
	}
}