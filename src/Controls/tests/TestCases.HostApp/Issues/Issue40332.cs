using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40332, "[CollectionView] ItemTapped event - Parity with ListView", PlatformAffected.All)]
public class Issue40332 : ContentPage
{
	readonly CollectionView _collectionView;
	readonly Label _eventLog;
	readonly List<string> _entries = new();
	readonly List<string> _items = new() { "A", "B", "C" };
	readonly List<string> _duplicates = new() { "A", "B", "A" };
	readonly List<string> _plainItems = new() { "Plain1", "Plain2" };
	readonly DataTemplate _itemTemplate;
	readonly List<Issue40332Group> _groups = new()
	{
		new Issue40332Group("G1", new[] { "A", "B" }),
		new Issue40332Group("G2", new[] { "C", "D", "E" }),
	};

	public Issue40332()
	{
		_eventLog = new Label { AutomationId = "EventLog", Text = "Log:" };

		_collectionView = new CollectionView
		{
			AutomationId = "TapCollectionView",
			SelectionMode = SelectionMode.Single,
			ItemsSource = _items,
			ItemTemplate = _itemTemplate = new DataTemplate(() =>
			{
				var label = new Label { FontSize = 20, VerticalOptions = LayoutOptions.Center };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "Item{0}"));

				var button = new Button { Text = "Inner" };
				button.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "Inner{0}"));
				button.Clicked += (s, e) => Log($"Inner:{((Button)s).BindingContext}");

				var grid = new Grid
				{
					Padding = new Thickness(12),
					ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
					Children = { label, button }
				};
				grid.SetColumn(button, 1);
				return grid;
			}),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var header = new Label { FontAttributes = FontAttributes.Bold, BackgroundColor = Colors.LightGray, Padding = new Thickness(12, 4) };
				header.SetBinding(Label.TextProperty, nameof(Issue40332Group.Name));
				return header;
			}),
		};

		_collectionView.SelectionChanged += (s, e) =>
			Log($"Selection:{string.Join(",", e.CurrentSelection)}");
		_collectionView.ItemTapped += OnItemTapped;

		Grid.SetRow(_eventLog, 1);
		Grid.SetRow(_collectionView, 2);

		Content = new Grid
		{
			RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) },
			Children =
			{
				new VerticalStackLayout
				{
					Spacing = 4,
					Children =
					{
						new HorizontalStackLayout
						{
							Spacing = 4,
							Children =
							{
								CreateButton("ModeNone", () => _collectionView.SelectionMode = SelectionMode.None),
								CreateButton("ModeSingle", () => _collectionView.SelectionMode = SelectionMode.Single),
								CreateButton("ModeMultiple", () => _collectionView.SelectionMode = SelectionMode.Multiple),
							}
						},
						new HorizontalStackLayout
						{
							Spacing = 4,
							Children =
							{
								CreateButton("Grouped", () =>
								{
									_collectionView.IsGrouped = true;
									_collectionView.ItemsSource = _groups;
								}),
								CreateButton("Unsubscribe", () => _collectionView.ItemTapped -= OnItemTapped),
								CreateButton("Reset", Reset),
							}
						},
						new HorizontalStackLayout
						{
							Spacing = 4,
							Children =
							{
								CreateButton("Duplicates", () => _collectionView.ItemsSource = _duplicates),
								CreateButton("NoTemplate", () =>
								{
									_collectionView.ItemTemplate = null;
									_collectionView.ItemsSource = _plainItems;
								}),
								CreateButton("UseCommand", () =>
								{
									_collectionView.ItemTapped -= OnItemTapped;
									_collectionView.ItemTappedCommand = new Command(p => Log($"Command:{p}"));
								}),
							}
						},
					}
				},
				_eventLog,
				_collectionView,
			}
		};
	}

	void Reset()
	{
		_collectionView.ItemTapped -= OnItemTapped;
		_collectionView.ItemTapped += OnItemTapped;
		_collectionView.ItemTappedCommand = null;
		_collectionView.ItemTemplate = _itemTemplate;
		_collectionView.SelectionMode = SelectionMode.None;
		_collectionView.SelectedItem = null;
		_collectionView.IsGrouped = false;
		_collectionView.ItemsSource = _items;
		_collectionView.SelectionMode = SelectionMode.Single;
		_entries.Clear();
		_eventLog.Text = "Log:";
	}

	void OnItemTapped(object sender, ItemTappedEventArgs e)
	{
		var group = e.Group is Issue40332Group g ? $"/{g.Name}" : string.Empty;
		Log($"Tapped:{e.Item}@{e.ItemIndex}{group}");
	}

	void Log(string entry)
	{
		_entries.Add(entry);
		_eventLog.Text = "Log:" + string.Join(";", _entries);
	}

	static Button CreateButton(string automationId, Action action)
	{
		var button = new Button { Text = automationId, AutomationId = automationId, Padding = new Thickness(6, 2) };
		button.Clicked += (s, e) => action();
		return button;
	}
}

public class Issue40332Group : ObservableCollection<string>
{
	public Issue40332Group(string name, IEnumerable<string> items) : base(items)
	{
		Name = name;
	}

	public string Name { get; }
}
