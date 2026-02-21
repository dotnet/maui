using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32139, "CarouselView scrolls to last item when Loop is enabled and CurrentItem is set to an item not in the list", PlatformAffected.iOS)]
public class Issue32139 : ContentPage
{
	const string InitialItem = "0";
	const string InvalidItem = "14";

	Label selectedItemLabel;
	CarouselView carouselView;
	public Issue32139()
	{
		carouselView = new CarouselView
		{
			HeightRequest = 150,
			Loop = true,
			ItemsSource = new ObservableCollection<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" },
			ItemTemplate = new DataTemplate(() =>
			{
				Border border = new Border
				{
					BackgroundColor = Colors.LightGray,
					Content = new Label
					{
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						FontSize = 80
					}
				};
				border.Content.SetBinding(Label.TextProperty, ".");
				return border;
			})
		};

		selectedItemLabel = new Label
		{
			Text = $"Selected Item : {InitialItem}",
			FontSize = 16
		};

		Button selectButton = new Button
		{
			AutomationId = "Issue32139SelectBtn",
			Text = $"Select item {InvalidItem}",
			FontSize = 12
		};

		selectButton.Clicked += (s, e) =>
		{
			carouselView.CurrentItem = InvalidItem;
			selectedItemLabel.Text = $"Selected Item : {carouselView.CurrentItem}";
		};

		Content = new VerticalStackLayout
		{
			Padding = 25,
			Spacing = 15,
			Children =
			{
				carouselView,
				selectedItemLabel,
				selectButton,
			}
		};
	}
}