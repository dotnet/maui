using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38925, "CollectionView with GridItemsLayout: SpacingItemDecoration walks all positions for every cell", PlatformAffected.Android)]
public class Issue38925 : TestContentPage
{
	const int GroupCount = 50;
	const int ItemsPerGroup = 200;

	CollectionView _collectionView;
	Label _statusLabel;
	Label _timingLabel;
	ObservableCollection<Issue38925Group> _groups;

	protected override void Init()
	{
		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Ready"
		};

		_timingLabel = new Label
		{
			AutomationId = "TimingLabel",
			Text = string.Empty
		};

		var loadButton = new Button
		{
			AutomationId = "LoadButton",
			Text = $"Load {GroupCount * ItemsPerGroup} grouped items"
		};

		var scrollToEndButton = new Button
		{
			AutomationId = "ScrollToEndButton",
			Text = "Scroll to end"
		};

		var scrollToStartButton = new Button
		{
			AutomationId = "ScrollToStartButton",
			Text = "Scroll to start"
		};

		loadButton.Clicked += (_, _) =>
		{
			_groups = new ObservableCollection<Issue38925Group>(
				Enumerable.Range(1, GroupCount).Select(g =>
					new Issue38925Group($"Group {g}", Enumerable.Range(1, ItemsPerGroup).Select(i => $"G{g} Item {i}"))));

			MeasureUiThreadBlock("Loaded", () => _collectionView.ItemsSource = _groups);
		};

		scrollToEndButton.Clicked += (_, _) =>
		{
			if (_groups is null)
			{
				return;
			}

			var lastGroup = _groups.Count - 1;

			MeasureUiThreadBlock("Scrolled to end", () =>
				_collectionView.ScrollTo(_groups[lastGroup].Count - 1, lastGroup, ScrollToPosition.End, animate: false));
		};

		scrollToStartButton.Clicked += (_, _) =>
		{
			if (_groups is null)
			{
				return;
			}

			MeasureUiThreadBlock("Scrolled to start", () =>
				_collectionView.ScrollTo(0, 0, ScrollToPosition.Start, animate: false));
		};

		_collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			IsGrouped = true,
			ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
			{
				HorizontalItemSpacing = 4,
				VerticalItemSpacing = 4
			},
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = 8,
					FontAttributes = FontAttributes.Bold
				};
				label.SetBinding(Label.TextProperty, nameof(Issue38925Group.Name));
				return label;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 11
				};
				label.SetBinding(Label.TextProperty, ".");

				return new Grid
				{
					HeightRequest = 60,
					BackgroundColor = Colors.LightGray,
					Children = { label }
				};
			})
		};

		var buttons = new HorizontalStackLayout
		{
			Spacing = 8,
			Children = { loadButton, scrollToEndButton, scrollToStartButton }
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			RowSpacing = 4
		};

		grid.Add(buttons, 0, 0);
		grid.Add(_statusLabel, 0, 1);
		grid.Add(_timingLabel, 0, 2);
		grid.Add(_collectionView, 0, 3);

		Content = grid;
	}

	// The callback cannot run until the layout pass triggered by `action` has finished,
	// so the elapsed time is how long the UI thread was blocked.
	void MeasureUiThreadBlock(string status, Action action)
	{
		_statusLabel.Text = "Working";
		var stopwatch = Stopwatch.StartNew();

		action();

		Dispatcher.Dispatch(() =>
		{
			stopwatch.Stop();
			_timingLabel.Text = $"{status}: UI thread blocked {stopwatch.ElapsedMilliseconds} ms";
			_statusLabel.Text = status;
		});
	}
}

// Items are unique across groups: grouped ScrollTo resolves its target by item equality.
public class Issue38925Group : ObservableCollection<string>
{
	public string Name { get; }

	public Issue38925Group(string name, IEnumerable<string> items) : base(items)
	{
		Name = name;
	}
}
