namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5831, "Navigating away from CollectionView and coming back leaves weird old items", PlatformAffected.iOS)]
public class Issue5831 : TestShell
{
	const string flyoutMainTitle = "Main";
	const string flyoutOtherTitle = "Other Page";

	protected override void Init()
	{

		Tab otherFlyoutItem = new Tab();
		Tab mainFlyoutItem = new Tab();

		string[] items = {
							"Baboon",
							"Capuchin Monkey",
							"Blue Monkey",
							"Squirrel Monkey",
							"Golden Lion Tamarin",
							"Howler Monkey",
							"Japanese Macaque",
						};
#pragma warning disable CS0618 // Type or member is obsolete
		var collectionView = new CollectionView() { VerticalOptions = LayoutOptions.FillAndExpand };
#pragma warning restore CS0618 // Type or member is obsolete
		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			return label;
		});
		collectionView.ItemsSource = items;
#pragma warning disable CS0618 // Type or member is obsolete
		var stackLayout = new StackLayout() { VerticalOptions = LayoutOptions.FillAndExpand };
#pragma warning restore CS0618 // Type or member is obsolete
		stackLayout.Children.Add(new Label { Text = "Go to the other page via the flyout, then come back. The items in the collection view should look identical when you return to this page." });
		stackLayout.Children.Add(collectionView);
		var collectionViewPage = new ContentPage { Content = stackLayout, BindingContext = this };
		mainFlyoutItem.Items.Add(collectionViewPage);

		otherFlyoutItem.Items.Add(new ContentPage { Content = new Label { Text = "Go back to main page via the flyout", AutomationId = "Label" } });

		Items.Add(new FlyoutItem
		{
			Title = flyoutMainTitle,
			Items = { mainFlyoutItem }
		});

		Items.Add(new FlyoutItem
		{
			Title = flyoutOtherTitle,
			Items = { otherFlyoutItem }
		});

	}
}
