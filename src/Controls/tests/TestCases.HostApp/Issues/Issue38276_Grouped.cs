namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38276, "Grouped CollectionView does not update its rendered height after ItemsSource changes", PlatformAffected.iOS | PlatformAffected.macOS, issueTestNumber: 1)]
public class Issue38276_Grouped : ContentPage
{
	const double MaximumCollectionHeight = 240;

	public Issue38276_Grouped()
	{
		CollectionView collectionView = new CollectionView
		{
			AutomationId = "GroupedCollectionView",
			Background = Colors.LightGrey,
			MaximumHeightRequest = MaximumCollectionHeight,
			VerticalOptions = LayoutOptions.Start,
			IsGrouped = true,
			ItemsSource = CreateGroups(3, 6),
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontFamily = "OpenSansRegular",
					FontSize = 13,
					FontAttributes = FontAttributes.Bold,
					LineBreakMode = LineBreakMode.TailTruncation
				};
				label.SetBinding(Label.TextProperty, ".");

				return new Border
				{
					StrokeThickness = 0,
					Padding = new Thickness(8, 4),
					Margin = new Thickness(0, 2),
					Content = label
				};
			}),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontAttributes = FontAttributes.Bold,
					Padding = new Thickness(8, 4)
				};
				label.SetBinding(Label.TextProperty, nameof(Issue38276Group.Header));
				return label;
			}),
			GroupFooterTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontAttributes = FontAttributes.Italic,
					Padding = new Thickness(8, 4)
				};
				label.SetBinding(Label.TextProperty, nameof(Issue38276Group.Footer));
				return label;
			})
		};

		Button largeGroupsButton = new Button
		{
			AutomationId = "LargeGroupsButton",
			Text = "Show large groups"
		};
		largeGroupsButton.Clicked += (sender, args) => collectionView.ItemsSource = CreateGroups(3, 6);

		Button smallGroupsButton = new Button
		{
			AutomationId = "SmallGroupsButton",
			Text = "Show small group"
		};
		smallGroupsButton.Clicked += (sender, args) => collectionView.ItemsSource = CreateGroups(1, 1);

		var layout = new Grid
		{
			Padding = 20,
			RowSpacing = 8,
			RowDefinitions =
			[
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			]
		};

		layout.Add(collectionView, row: 0);
		layout.Add(smallGroupsButton, row: 1);
		layout.Add(largeGroupsButton, row: 2);

		Content = layout;
	}

	static List<Issue38276Group> CreateGroups(int groupCount, int itemCount) =>
		Enumerable.Range(1, groupCount)
			.Select(groupIndex => new Issue38276Group(
				$"Group {groupIndex} header",
				$"Group {groupIndex} footer",
				Enumerable.Range(1, itemCount).Select(itemIndex => $"Item {groupIndex}.{itemIndex}")))
			.ToList();
}

public class Issue38276Group(string header, string footer, IEnumerable<string> items) : List<string>(items)
{
	public string Header { get; } = header;

	public string Footer { get; } = footer;
}