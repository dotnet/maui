namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29634, "iOS CV: Empty view not resizing when bounds change", PlatformAffected.iOS)]
public class Issue29634 : ContentPage
{
	CollectionView _collectionView;
	public Issue29634()
	{
		Grid grid = null;
		var button = new Button
		{
			Margin = new Thickness(0, 0, 0, 5),
			BackgroundColor = Colors.LightSeaGreen,
			FontSize = 12,
			TextColor = Colors.DarkSlateGray,
			Text = "Button text",
			Command = new Command(() => grid.WidthRequest = 200),
			AutomationId = "RunTest"
		};

		button.SizeChanged += async (sender, e) =>
		{
			await Task.Yield(); // Ensure the layout pass is complete before checking size
			if (sender is Button b && Content is VerticalStackLayout l && grid.WidthRequest == 200)
			{
				if (l.Children.Count > 1)
					l.Children.RemoveAt(1); // Remove the previous label if it exists

				if (b.Width == 200)
					l.Add(new Label() { Text = "Button Successfully resized", AutomationId = "SuccessLabel" });
				else
					l.Add(new Label() { Text = $"Button Failed To Resize to 200: {b.Width}x{b.Height}", AutomationId = "FailLabel" });
			}
		};

		_collectionView = new CollectionView
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 10 },
			ItemTemplate = new DataTemplate(),
			EmptyView = button
		};

		grid = new Grid
		{
			WidthRequest = 400,
			HeightRequest = 200,
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)
			],
			Children = { _collectionView }
		};

		Grid.SetColumn(_collectionView, 1);

		Content = new VerticalStackLayout
		{
			Children =
			{
				grid
			}
		};
	}
}