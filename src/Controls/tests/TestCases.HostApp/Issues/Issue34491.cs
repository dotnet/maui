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

		var pointerStatusLabel = new Label
		{
			Text = "No Pointer Events",
			AutomationId = "PointerStatusLabel",
			Padding = new Thickness(10),
			FontSize = 16
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

				pointerGesture.PointerPressed += (s, e) =>
					pointerStatusLabel.Text = $"Pointer Pressed: {grid.BindingContext}";

				pointerGesture.PointerReleased += (s, e) =>
					pointerStatusLabel.Text = $"Pointer Released: {grid.BindingContext}";

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


		var mixedSelectionStatusLabel = new Label
		{
			Text = "No Mixed Selection",
			AutomationId = "MixedSelectionStatusLabel",
			Padding = new Thickness(10),
			FontSize = 18
		};

		var mixedTapStatusLabel = new Label
		{
			Text = "No Mixed Tap",
			AutomationId = "MixedTapStatusLabel",
			Padding = new Thickness(10),
			FontSize = 16
		};

		var mixedCollectionView = new CollectionView
		{
			AutomationId = "MixedTestCollectionView",
			SelectionMode = SelectionMode.Single,
			ItemsSource = new List<string> { "Mixed Item 1", "Mixed Item 2", "Mixed Item 3" },
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
					BackgroundColor = Colors.LightBlue,
					Padding = new Thickness(10),
					HeightRequest = 50,
					Children = { label }
				};

				var pointerGesture = new PointerGestureRecognizer();

				pointerGesture.PointerPressed += (s, e) =>
					pointerStatusLabel.Text = $"Pointer Pressed: {grid.BindingContext}";

				pointerGesture.PointerReleased += (s, e) =>
					pointerStatusLabel.Text = $"Pointer Released: {grid.BindingContext}";

				grid.GestureRecognizers.Add(pointerGesture);

				var tapGesture = new TapGestureRecognizer();
				tapGesture.Tapped += (s, e) =>
					mixedTapStatusLabel.Text = $"Tapped: {grid.BindingContext}";

				grid.GestureRecognizers.Add(tapGesture);

				return grid;
			})
		};

		mixedCollectionView.SelectionChanged += (s, e) =>
		{
			if (e.CurrentSelection.Count > 0)
			{
				mixedSelectionStatusLabel.Text = $"Mixed Selected: {e.CurrentSelection[0]}";
			}
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(10),
			Spacing = 10,
			Children =
			{
				statusLabel,
				pointerStatusLabel,
				collectionView,

				mixedSelectionStatusLabel,
				mixedTapStatusLabel,
				mixedCollectionView
			}
		};
	}
}
