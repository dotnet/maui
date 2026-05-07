namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35326, "CollectionView.ScrollTo(index) doesn't work correctly when IsGrouped=\"True\" on iOS, MacCatalyst, and Windows", PlatformAffected.All)]
public class Issue35326 : ContentPage
{
	public Issue35326()
	{
		var groups = new List<Issue35326Group>();
		for (int g = 1; g <= 5; g++)
		{
			var group = new Issue35326Group { Key = $"Group {g}" };
			for (int i = 1; i <= 10; i++)
			{
				group.Add(new Issue35326Item
				{
					Name = $"Group {g} — Item {i}",
					AutoId = $"Item_G{g}_I{i}"
				});
			}
			groups.Add(group);
		}

		var collectionView = new CollectionView
		{
			AutomationId = "GroupedCollectionView",
			IsGrouped = true,
			ItemsSource = groups,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(8, 4),
					BackgroundColor = Colors.LightGray,
					FontAttributes = FontAttributes.Bold
				};
				label.SetBinding(Label.TextProperty, "Key");
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = new Thickness(16, 8) };
				label.SetBinding(Label.TextProperty, "Name");
				label.SetBinding(Label.AutomationIdProperty, "AutoId");
				return label;
			})
		};

		var scrollToEndButton = new Button
		{
			Text = "Scroll To End",
			AutomationId = "ScrollToEndButton",
			Command = new Command(() => collectionView.ScrollTo(40, position: ScrollToPosition.Start, animate: false))
		};

		var scrollToStartButton = new Button
		{
			Text = "Scroll To Start",
			AutomationId = "ScrollToStartButton",
			Command = new Command(() => collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false))
		};

		var scrollToItemButton = new Button
		{
			Text = "Scroll To Item",
			AutomationId = "ScrollToItemButton",
			Command = new Command(() =>
			{
				var lastGroup = groups[^1];
				var lastItem = lastGroup[^1];
				collectionView.ScrollTo(lastItem, lastGroup, position: ScrollToPosition.End, animate: false);
			})
		};

		var buttonPanel = new HorizontalStackLayout
		{
			Padding = new Thickness(8),
			Spacing = 8,
			Children = { scrollToStartButton, scrollToEndButton, scrollToItemButton }
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(buttonPanel, 0, 0);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}
}

public class Issue35326Item
{
	public string Name { get; set; } = string.Empty;
	public string AutoId { get; set; } = string.Empty;
}

public class Issue35326Group : List<Issue35326Item>
{
	public string Key { get; set; } = string.Empty;
}
