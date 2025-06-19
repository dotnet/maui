namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29634, "iOS CV: Empty view not resizing when bounds change", PlatformAffected.iOS)]
public class Issue29634 : ContentPage
{
	CollectionView _collectionView;
	public Issue29634()
	{
		var button = new Button
		{
			Margin = new Thickness(0, 0, 0, 5),
			BackgroundColor = Colors.LightSeaGreen,
			FontSize = 12,
			TextColor = Colors.DarkSlateGray,
			Text = "Button text",
			Command = new Command(() => Content.WidthRequest = 200),
			AutomationId = "RunTest"
		};

		button.SizeChanged += (sender, e) =>
		{
			if (sender is Button b && b.Width == 200)
			{
				((Grid)Content).Add(new Label(){Text = "Button Successfully resized", AutomationId = "SuccessLabel"});
			}
		};

		_collectionView = new CollectionView
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 10 },
			ItemTemplate = new DataTemplate(),
			EmptyView = button
		};

		Grid.SetColumn(_collectionView, 1);

		Content = new Grid
		{
			WidthRequest = 400,
			ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Auto),
				new ColumnDefinition(GridLength.Star)
			],
			Children = { _collectionView }
		};
	}
}