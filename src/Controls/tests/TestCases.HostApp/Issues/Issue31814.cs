namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31814, "RemainingItemsThresholdReached is not raised for a single item when the CollectionView has a Header", PlatformAffected.Android)]
public class Issue31814 : ContentPage
{
	public Issue31814()
	{
		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Waiting"
		};

		var offsetLabel = new Label
		{
			AutomationId = "OffsetLabel",
			Text = "0"
		};

		var collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			RemainingItemsThreshold = 0,
			ItemsSource = new[] { "Item 0" },
			// Taller than the screen, so the only item shows up after scrolling
			Header = new Label { Text = "Header", HeightRequest = 1500 },
			Footer = new Label { Text = "Footer", AutomationId = "FooterLabel", HeightRequest = 100 },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { HeightRequest = 100 };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		collectionView.RemainingItemsThresholdReached += (s, e) => statusLabel.Text = "Threshold reached";
		collectionView.Scrolled += (s, e) => offsetLabel.Text = $"{e.VerticalOffset}";

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};
		grid.Add(statusLabel, 0, 0);
		grid.Add(offsetLabel, 0, 1);
		grid.Add(collectionView, 0, 2);
		Content = grid;
	}
}
