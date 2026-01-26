using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29420, "KeepLastInView Not Working as Expected in CarouselView", PlatformAffected.UWP)]
public class Issue29420 : ContentPage
{
	public Issue29420()
	{
		var count = 0;
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"Item 0", "Item 1","Item 2", "Item 3", "Item 4", "Item 5",

		};

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "CarouselView",
			Loop = false,
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,

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
					Background = Colors.Pink,
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				grid.Children.Add(label);
				return grid;
			}),
		};

		var insertButton = new Button
		{
			Text = "Insert item at 0th index",
			AutomationId = "InsertButton",
			Margin = new Thickness(20),
		};
		
		var addButton = new Button
		{
			Text = "Add item at end",
			AutomationId = "AddButton",
			Margin = new Thickness(20),
		};

		insertButton.Clicked += (sender, e) =>
		{
			carouselItems.Insert(0, "NewItem" + count.ToString());
			count++;
		};

		addButton.Clicked += (sender, e) =>
		{
			carouselItems.Add("NewItem");
		};

		verticalStackLayout.Children.Add(insertButton);
		verticalStackLayout.Children.Add(addButton);
		verticalStackLayout.Children.Add(carouselView);
		Content = verticalStackLayout;
	}
}