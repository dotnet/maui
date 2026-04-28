namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19866, "CollectionView does not scroll to top on iOS status bar tap", PlatformAffected.iOS)]
public class Issue19866 : TestShell
{
	protected override void Init()
	{
		var items = Enumerable.Range(0, 100).Select(i => $"Item {i}").ToList();

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Margin = new Thickness(10, 20),
					FontSize = 16
				};
				label.SetBinding(Label.TextProperty, new Binding("."));
				label.SetBinding(Label.AutomationIdProperty, new Binding("."));
				return label;
			})
		};

		AddContentPage(new ContentPage
		{
			Title = "Issue 19866",
			Content = collectionView
		});
	}
}
