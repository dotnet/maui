namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28678, "TargetInvocationException Occurs When Selecting Header/Footer After Changing ItemsLayout in CV2", PlatformAffected.iOS)]
public class Issue28678 : ContentPage
{
	public Issue28678()
	{
		// Create the CollectionView
		var collectionView = new CollectionView2
		{
			HeightRequest = 100,
		};

		var button = new Button
		{
			Text = "Change ItemsLayout",
			AutomationId = "Button"
		};
		button.Clicked += (s, e) =>
		{
			collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
		};

		var button1 = new Button
		{
			Text = "Add Header",
			AutomationId = "HeaderButton"
		};
		button1.Clicked += (s, e) =>
		{
			collectionView.Header = "Header";
		};

		var button2 = new Button
		{
			Text = "Add Footer",
			AutomationId = "FooterButton"
		};
		button2.Clicked += (s, e) =>
		{
			collectionView.Footer = "Footer";
		};

		Content = new VerticalStackLayout
		{
			Children =
			{
				collectionView,
				button,
				button1,
				button2
			}
		};
	}
}