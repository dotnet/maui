namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19667, "CollectionView contents not sizing correctly after orientation change", PlatformAffected.Android)]
public class Issue19667 : TestShell
{
	protected override void Init()
	{
		ContentPage page1 = new ContentPage
		{
			Content = new Label
			{
				Text = "Page 1",
				AutomationId = "Page1Label",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		var items = Enumerable.Range(0, 15).ToList();

		CollectionView collectionView = new CollectionView
		{
			AutomationId = "CollectionView19667",
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, new Binding(".", stringFormat: "Item {0}"));
				label.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "CvItem{0}"));

				Grid grid = new Grid
				{
					BackgroundColor = Colors.Beige,
					Padding = new Thickness(20),
					Margin = new Thickness(5, 2)
				};
				grid.Add(label);
				return grid;
			})
		};

		ContentPage page2 = new ContentPage
		{
			Content = collectionView
		};

		AddFlyoutItem(page1, "Page1");
		AddFlyoutItem(page2, "CollectionViewPage");
	}
}
