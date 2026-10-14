namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38276, "CollectionView does not update its rendered height after ItemsSource changes", PlatformAffected.iOS | PlatformAffected.macOS, issueTestNumber: 0)]
public class Issue38276 : ContentPage
{
	const double MaximumCollectionHeight = 180;

	public Issue38276()
	{
		CollectionView collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			Background = Colors.LightGrey,
			MaximumHeightRequest = MaximumCollectionHeight,
			VerticalOptions = LayoutOptions.Start,
			ItemsSource = CreateItems(10),
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
			})
		};

		Button largeItemsButton = new Button
		{
			AutomationId = "LargeItemsButton",
			Text = "Show 10 items"
		};
		largeItemsButton.Clicked += (sender, args) => collectionView.ItemsSource = CreateItems(10);

		Button singleItemButton = new Button
		{
			AutomationId = "SingleItemButton",
			Text = "Show 1 item"
		};
		singleItemButton.Clicked += (sender, args) => collectionView.ItemsSource = CreateItems(1);

		Button emptyItemsButton = new Button
		{
			AutomationId = "EmptyItemsButton",
			Text = "Show no items"
		};
		emptyItemsButton.Clicked += (sender, args) => collectionView.ItemsSource = CreateItems(0);

		Label afterCollectionLabel = new Label
		{
			AutomationId = "AfterCollectionLabel",
			Text = "After collection"
		};

		var layout = new Grid
		{
			Padding = 20,
			RowSpacing = 8,
			RowDefinitions =
			[
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			]
		};

		layout.Add(collectionView, row: 0);
		layout.Add(afterCollectionLabel, row: 1);
		layout.Add(emptyItemsButton, row: 2);
		layout.Add(singleItemButton, row: 3);
		layout.Add(largeItemsButton, row: 4);

		Content = layout;
	}

	static List<string> CreateItems(int count) =>
		Enumerable.Range(1, count).Select(index => $"Item {index}").ToList();
}