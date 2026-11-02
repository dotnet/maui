using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38926, "CollectionView GridItemsLayout: span lookups over uncached positions are O(position) per cell", PlatformAffected.Android)]
public class Issue38926 : TestContentPage
{
	const int GroupCount = 200;
	const int ItemsPerGroup = 1000;

	CollectionView _collectionView;
	Label _statusLabel;
	Label _timingLabel;
	ObservableCollection<Issue38926Group> _groups;
	List<string> _flatItems;

	protected override void Init()
	{
		_statusLabel = new Label { AutomationId = "StatusLabel", Text = "Ready" };
		_timingLabel = new Label { AutomationId = "TimingLabel", Text = string.Empty };

		var loadGroupedButton = new Button { AutomationId = "LoadGroupedButton", Text = "Load grouped" };
		var loadFlatButton = new Button { AutomationId = "LoadFlatButton", Text = "Load flat" };
		var scrollToEndButton = new Button { AutomationId = "ScrollToEndButton", Text = "To end" };
		var scrollToMiddleButton = new Button { AutomationId = "ScrollToMiddleButton", Text = "To middle" };
		var scrollToStartButton = new Button { AutomationId = "ScrollToStartButton", Text = "To start" };

		_groups = new ObservableCollection<Issue38926Group>(
			Enumerable.Range(1, GroupCount).Select(g =>
				new Issue38926Group($"Group {g}", Enumerable.Range(1, ItemsPerGroup).Select(i => $"G{g} Item {i}"))));
		_flatItems = _groups.SelectMany(g => g).ToList();

		loadGroupedButton.Clicked += (_, _) => MeasureUiThreadBlock("Loaded grouped", () =>
		{
			_collectionView.IsGrouped = true;
			_collectionView.ItemsSource = _groups;
		});

		loadFlatButton.Clicked += (_, _) => MeasureUiThreadBlock("Loaded flat", () =>
		{
			_collectionView.IsGrouped = false;
			_collectionView.ItemsSource = _flatItems;
		});

		scrollToEndButton.Clicked += (_, _) => MeasureUiThreadBlock("Scrolled to end", () => ScrollToFlatIndex(_flatItems.Count - 1, ScrollToPosition.End));
		scrollToMiddleButton.Clicked += (_, _) => MeasureUiThreadBlock("Scrolled to middle", () => ScrollToFlatIndex(_flatItems.Count / 2, ScrollToPosition.Center));
		scrollToStartButton.Clicked += (_, _) => MeasureUiThreadBlock("Scrolled to start", () => ScrollToFlatIndex(0, ScrollToPosition.Start));

		_collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 8, FontAttributes = FontAttributes.Bold };
				label.SetBinding(Label.TextProperty, nameof(Issue38926Group.Name));
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

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			RowSpacing = 4
		};

		grid.Add(new HorizontalStackLayout { Spacing = 8, Children = { loadGroupedButton, loadFlatButton } }, 0, 0);
		grid.Add(new HorizontalStackLayout { Spacing = 8, Children = { scrollToEndButton, scrollToMiddleButton, scrollToStartButton } }, 0, 1);
		grid.Add(_statusLabel, 0, 2);
		grid.Add(_timingLabel, 0, 3);
		grid.Add(_collectionView, 0, 4);

		Content = grid;
	}

	void ScrollToFlatIndex(int flatIndex, ScrollToPosition scrollToPosition)
	{
		if (_collectionView.ItemsSource is null)
		{
			return;
		}

		if (_collectionView.IsGrouped)
		{
			var groupIndex = flatIndex / ItemsPerGroup;
			_collectionView.ScrollTo(flatIndex % ItemsPerGroup, groupIndex, scrollToPosition, animate: false);
		}
		else
		{
			_collectionView.ScrollTo(flatIndex, -1, scrollToPosition, animate: false);
		}
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
public class Issue38926Group : ObservableCollection<string>
{
	public string Name { get; }

	public Issue38926Group(string name, IEnumerable<string> items) : base(items)
	{
		Name = name;
	}
}
