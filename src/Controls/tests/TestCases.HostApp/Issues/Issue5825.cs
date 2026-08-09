namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5825, "[Android] TapGestureRecognizer doesn't fire inside CollectionView/ListView", PlatformAffected.Android)]
public class Issue5825 : ContentPage
{
	public Issue5825()
	{
		var resultLabel = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabel",
			FontSize = 24,
			HorizontalOptions = LayoutOptions.Center
		};

		var items = new List<string> { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };

		var collectionView = new CollectionView
		{
			ItemsSource = items,
			HeightRequest = 300,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 18,
					AutomationId = "CollectionViewItem"
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		var tapGesture = new TapGestureRecognizer
		{
			NumberOfTapsRequired = 2,
			Command = new Command(() =>
			{
				resultLabel.Text = "Success";
			})
		};

		collectionView.GestureRecognizers.Add(tapGesture);

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label
				{
					Text = "Double-tap on the CollectionView below. The label should change to 'Success'.",
					FontSize = 16
				},
				collectionView,
				resultLabel
			}
		};
	}
}
