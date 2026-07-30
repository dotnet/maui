namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35313, "ScrollTo(0) not working on grouped CollectionView", PlatformAffected.Android)]
public class Issue35313 : ContentPage
{
	CollectionView _collectionView;
	List<Issue35313ItemGroup> _groups;

	public Issue35313()
	{
		_groups = [];
		for (int g = 1; g <= 5; g++)
		{
			var group = new Issue35313ItemGroup { Key = $"Group {g}" };
			for (int i = 1; i <= 10; i++)
				group.Add(new Issue35313Item { Name = $"Group {g} — Item {i}" });
			_groups.Add(group);
		}

		_collectionView = new CollectionView
		{
			AutomationId = "CollectionView",
			IsGrouped = true,
			ItemsSource = _groups,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(8, 4),
					BackgroundColor = Colors.LightGray,
					FontAttributes = FontAttributes.Bold
				};
				label.SetBinding(Label.TextProperty, "Key");
				label.SetBinding(Label.AutomationIdProperty, "Key");
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = new Thickness(16, 8) };
				label.SetBinding(Label.TextProperty, "Name");
				label.SetBinding(Label.AutomationIdProperty, "Name");
				return label;
			})
		};

		var buttonStart = new Button
		{
			Text = "Start",
			AutomationId = "ScrollToStartButton",
			Command = new Command(() => _collectionView.ScrollTo(0))
		};

		var buttonFirstItem = new Button
		{
			Text = "First item",
			AutomationId = "ScrollToFirstItemButton",
			Command = new Command(() =>
			{
				var firstGroup = _groups[0];
				_collectionView.ScrollTo(firstGroup[0], firstGroup, ScrollToPosition.Start, animate: true);
			})
		};

		var buttonLastItem = new Button
		{
			Text = "Last item",
			AutomationId = "ScrollToLastItemButton",
			Command = new Command(() =>
			{
				var lastGroup = _groups[^1];
				_collectionView.ScrollTo(lastGroup[^1], lastGroup, ScrollToPosition.End, animate: true);
			})
		};

		var buttonEnd = new Button
		{
			Text = "End",
			AutomationId = "ScrollToEndButton",
			Command = new Command(() => _collectionView.ScrollTo(49))
		};

		var buttonRow = new HorizontalStackLayout
		{
			Padding = new Thickness(8),
			Spacing = 8,
			Children = { buttonStart, buttonFirstItem, buttonLastItem, buttonEnd }
		};

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			},
			Children = { buttonRow, _collectionView }
		};

		Grid.SetRow(buttonRow, 0);
		Grid.SetRow(_collectionView, 1);
	}
}

public class Issue35313Item
{
	public string Name { get; set; } = string.Empty;
}

public class Issue35313ItemGroup : List<Issue35313Item>
{
	public string Key { get; set; } = string.Empty;
}
