using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18227, "CollectionView with Header Causes ArgumentOutOfRangeException on reordering to last index", PlatformAffected.Android)]
public class Issue18227 : ContentPage
{
	public Issue18227()
	{
		var collectionView = new CollectionView
		{
			CanReorderItems = true,
			CanMixGroups = true,
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

		Content = collectionView;
	}
}