namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23038, "[Windows] GroupHeaderTemplate width smaller than ItemTemplate (GridLayout)", PlatformAffected.UWP, issueTestNumber: 1)]
public class Issue23038_GridLayout : ContentPage
{
	public Issue23038_GridLayout()
	{
		var teams = new List<Issue23038Team>
		{
			new("TeamA", "Team A", new List<Issue23038Member>
			{
				new("Member 1"), new("Member 2"), new("Member 3"), new("Member 4"),
			}),
			new("TeamB", "Team B", new List<Issue23038Member>
			{
				new("Member 5"), new("Member 6"),
			}),
		};

		var collectionView = new CollectionView
		{
			AutomationId = "GroupedGridCollectionView",
			IsGrouped = true,
			ItemsSource = teams,
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.LightBlue,
					FontAttributes = FontAttributes.Bold,
					Padding = new Thickness(5),
				};
				label.SetBinding(Label.TextProperty, nameof(Issue23038Team.Name));
				label.SetBinding(Label.AutomationIdProperty, nameof(Issue23038Team.Key), stringFormat: "GridHeader{0}");
				return label;
			}),
			GroupFooterTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.LightCoral,
					Padding = new Thickness(5),
				};
				label.SetBinding(Label.TextProperty, "Count", stringFormat: "Total: {0}");
				label.SetBinding(Label.AutomationIdProperty, nameof(Issue23038Team.Key), stringFormat: "GridFooter{0}");
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(5, 2),
				};
				label.SetBinding(Label.TextProperty, nameof(Issue23038Member.Name));
				return label;
			}),
		};

		Content = collectionView;
	}
}
