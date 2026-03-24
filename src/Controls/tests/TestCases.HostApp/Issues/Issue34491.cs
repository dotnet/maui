namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34491, "CollectionView item selection not triggered when PointerGestureRecognizer is added inside ItemTemplate", PlatformAffected.Android)]
public class Issue34491 : ContentPage
{
	public Issue34491()
	{
		var statusLabel = new Label
		{
			Text = "No Selection",
			AutomationId = "StatusLabel",
			Padding = new Thickness(10),
			FontSize = 18
		};

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			SelectionMode = SelectionMode.Single,
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(10),
					FontSize = 16
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				var grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					Padding = new Thickness(10),
					HeightRequest = 50,
					Children = { label }
				};

				var pointerGesture = new PointerGestureRecognizer();
				pointerGesture.PointerEntered += (s, e) => { };
				pointerGesture.PointerExited += (s, e) => { };
				grid.GestureRecognizers.Add(pointerGesture);

				return grid;
			})
		};

		collectionView.SelectionChanged += (s, e) =>
		{
			if (e.CurrentSelection.Count > 0)
			{
				statusLabel.Text = $"Selected: {e.CurrentSelection[0]}";
			}
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(10),
			Spacing = 10,
			Children = { statusLabel, collectionView }
		};
	}
}
