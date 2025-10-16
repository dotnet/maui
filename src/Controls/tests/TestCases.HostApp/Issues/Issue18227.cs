namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18227, "CollectionView with Header Causes ArgumentOutOfRangeException on reordering to last index", PlatformAffected.Android)]
public class Issue18227 : ContentPage
{
	public Issue18227()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var collectionView = new CollectionView
		{
			CanReorderItems = true,
			CanMixGroups = true,
			Margin = new Thickness(50),
			AutomationId = "collectionview",

			Header = new Label { Text = "Header" },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");
				return label;
			})
		};
		collectionView.ItemsSource = new List<string>()
		{
			"Test label 1",
			"Test label 2",
			"Test label 3",
			"Test label 4",
			"Test label 5",
			"Test label 6",
		};

		verticalStackLayout.Children.Add(collectionView);
		Content = verticalStackLayout;
	}
}