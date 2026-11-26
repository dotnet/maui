namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31814, "RemainingItemsThresholdReached is not raised for an empty source with a Footer", PlatformAffected.Android, issueTestNumber: 1)]
public class Issue31814_EmptySource : ContentPage
{
	public Issue31814_EmptySource()
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
			ItemsSource = Array.Empty<string>(),
			// Taller than the screen, so the Footer can be scrolled
			Footer = new Label { Text = "Footer", HeightRequest = 1500 }
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
