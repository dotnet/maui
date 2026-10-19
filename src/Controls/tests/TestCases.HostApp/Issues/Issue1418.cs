namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 1418, "RefreshView preserves horizontal scroll position", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue1418 : ContentPage
{
	public Issue1418()
	{
		var items = Enumerable.Range(0, 30).Select(index => $"Item{index}").ToList();
		CollectionView collectionView = new CollectionView
		{
			AutomationId = "HorizontalCollectionView",
			ItemsSource = items,
			BackgroundColor = Colors.LightGray,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					WidthRequest = 100,
					HeightRequest = 100,
					HorizontalTextAlignment = TextAlignment.Center,
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, ".");
				return label;
			})
		};

		RefreshView refreshView = new RefreshView
		{
			Content = collectionView,
			HeightRequest = 160,
			HorizontalOptions = LayoutOptions.Center
		};

		Label statusLabel = new Label { AutomationId = "RefreshStatus" };
		statusLabel.SetBinding(Label.TextProperty,
			new Binding(nameof(RefreshView.IsRefreshing), source: refreshView, stringFormat: "IsRefreshing: {0}"));

		Button scrollToEndButton = new Button
		{
			AutomationId = "ScrollToEnd",
			Text = "Scroll to end"
		};
		scrollToEndButton.Clicked += (_, _) =>
			collectionView.ScrollTo(items.Count - 1);

		Button toggleRefreshButton = new Button
		{
			AutomationId = "ToggleRefresh",
			Text = "Toggle refresh"
		};
		toggleRefreshButton.Clicked += (_, _) =>
			refreshView.IsRefreshing = !refreshView.IsRefreshing;

		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Children =
			{
				scrollToEndButton,
				toggleRefreshButton,
				statusLabel,
				refreshView
			}
		};
	}
}
