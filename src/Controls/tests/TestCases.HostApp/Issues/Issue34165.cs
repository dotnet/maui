namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34165, "CollectionView is scrolling left/right when the collection is empty and inside a RefreshView", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34165 : ContentPage
{
	public const string CollectionViewId = "CollectionView";
	public const string EmptyViewLabelId = "EmptyViewLabel";
	public const string RefreshViewId = "RefreshView";

	public Issue34165()
	{
		var emptyViewLabel = new Label
		{
			Text = "No items — swipe left/right here to test",
			AutomationId = EmptyViewLabelId,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var collectionView = new CollectionView
		{
			AutomationId = CollectionViewId,
			EmptyView = emptyViewLabel,
		};

		var refreshView = new RefreshView
		{
			AutomationId = RefreshViewId,
			Content = collectionView,
		};

		refreshView.Refreshing += (s, e) =>
		{
			((RefreshView)s!).IsRefreshing = false;
		};

		Content = refreshView;
	}
}
