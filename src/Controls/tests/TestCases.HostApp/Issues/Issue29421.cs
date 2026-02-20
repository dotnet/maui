using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29421, "KeepScrollOffset Not Working as Expected in CarouselView", PlatformAffected.UWP)]
public class Issue29421 : ContentPage
{
	public Issue29421()
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

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
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

		var indicatorView = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		carouselView.IndicatorView = indicatorView;

		var addButton = new Button
		{
			Text = "Add item",
			AutomationId = "AddButton",
			Margin = new Thickness(20),
		};

		addButton.Clicked += (sender, e) =>
		{
			carouselItems.Insert(0, "NewItem");
		};

		verticalStackLayout.Children.Add(carouselView);
		verticalStackLayout.Children.Add(indicatorView);
		verticalStackLayout.Children.Add(addButton);
		Content = verticalStackLayout;
	}
}