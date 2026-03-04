namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29472, "ItemsSource is not dynamically cleared in the CarouselView", PlatformAffected.UWP)]
public class Issue29472 : ContentPage
{
	public Issue29472()
	{
		Label descriptionLabel = new Label
		{
			Text = "The test passes if the previously bound items are cleared from the view when the ItemsSource is dynamically set to null; otherwise, it fails.",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center
		};

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = new List<string> { "Item1", "Item2" },
			Loop = false,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontSize = 24,
					BackgroundColor = Colors.LightGray,
					Margin = new Thickness(10)
				};

				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
		};

		Button clearItemsSource = new Button
		{
			AutomationId = "ClearItemsSourceBtn",
			Text = "Set the ItemsSource of the CarouselView to null",
			HorizontalOptions = LayoutOptions.Center
		};

		clearItemsSource.Clicked += (s, e) =>
		{
			carouselView.ItemsSource = null;
		};

		Grid grid = new Grid
		{
			Padding = 20,
			RowSpacing = 15,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		grid.Add(descriptionLabel, 0, 0);
		grid.Add(clearItemsSource, 0, 1);
		grid.Add(carouselView, 0, 2);

		Content = grid;
	}
}