using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38925, "CollectionView with GridItemsLayout: SpacingItemDecoration walks all positions for every cell", PlatformAffected.Android)]
public class Issue38925 : TestContentPage
{
	const int GroupCount = 50;
	const int ItemsPerGroup = 200;

	protected override void Init()
	{
		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Ready"
		};

		var timingLabel = new Label
		{
			AutomationId = "TimingLabel",
			Text = string.Empty
		};

		var loadButton = new Button
		{
			AutomationId = "LoadButton",
			Text = $"Load {GroupCount * ItemsPerGroup} grouped items"
		};

		var collectionView = new CollectionView
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
				label.SetBinding(Label.AutomationIdProperty, ".");

				return new Grid
				{
					HeightRequest = 60,
					BackgroundColor = Colors.LightGray,
					Children = { label }
				};
			})
		};

		loadButton.Clicked += (_, _) =>
		{
			var groups = new ObservableCollection<Issue38925Group>(
				Enumerable.Range(1, GroupCount).Select(g =>
					new Issue38925Group($"Group {g}", Enumerable.Range(1, ItemsPerGroup).Select(i => $"G{g} Item {i}"))));

			statusLabel.Text = "Working";
			var stopwatch = Stopwatch.StartNew();

			collectionView.ItemsSource = groups;

			// Include the delay until the UI thread can process another dispatcher callback.
			Dispatcher.Dispatch(() =>
			{
				stopwatch.Stop();
				timingLabel.Text = stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
				statusLabel.Text = "Loaded";
			});
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

		grid.Add(loadButton, 0, 0);
		grid.Add(statusLabel, 0, 1);
		grid.Add(timingLabel, 0, 2);
		grid.Add(collectionView, 0, 3);

		Content = grid;
	}
}

public class Issue38925Group : ObservableCollection<string>
{
	public string Name { get; }

	public Issue38925Group(string name, IEnumerable<string> items) : base(items)
	{
		Name = name;
	}
}
