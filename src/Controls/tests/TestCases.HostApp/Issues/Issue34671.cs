using System.Globalization;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34671, "[Windows] ScrollView offsets do not preserve when Orientation changes to Neither", PlatformAffected.UWP)]
public class Issue34671 : ContentPage
{
	const double ReproScrollX = 700;
	const double ReproScrollY = 480;
	readonly Label _orientationLabel;
	readonly Label _offsetLabel;
	readonly Label _lastActionLabel;
	readonly ScrollView _bugScrollView;
	string _lastAction = string.Empty;

	public Issue34671()
	{
		Title = "ScrollView Orientation Repro";

		_orientationLabel = new Label
		{
			AutomationId = "OrientationLabel",
			Text = "Orientation: Both"
		};

		_offsetLabel = new Label
		{
			AutomationId = "OffsetLabel",
			Text = "ScrollX: 0 | ScrollY: 0"
		};

		_lastActionLabel = new Label
		{
			AutomationId = "LastActionLabel",
			Text = "Action: launch the app and tap 'Scroll To Repro Offset'."
		};

		_bugScrollView = new ScrollView
		{
			AutomationId = "BugScrollView",
			Orientation = ScrollOrientation.Both,
			Content = CreateScrollContent()
		};
		_bugScrollView.Scrolled += OnScrollViewScrolled;

		Content = new Grid
		{
			Padding = 16,
			RowSpacing = 12,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				new Label
				{
					Text = "Repro for dotnet/maui#34671: scroll away from the origin, then set Orientation to Neither. On iOS, ScrollX and ScrollY reset to 0 instead of being preserved.",
					LineBreakMode = LineBreakMode.WordWrap
				}.Row(0),
				new VerticalStackLayout
				{
					Spacing = 6,
					Children =
					{
						new HorizontalStackLayout
						{
							Spacing = 8,
							Children =
							{
								new Button
								{
									Text = "Scroll To Repro Offset",
									AutomationId = "ScrollToReproOffsetButton"
								}.Assign(out var scrollToReproOffsetButton),
								new Button
								{
									Text = "Set Both",
									AutomationId = "SetBothButton"
								}.Assign(out var setBothButton),
								new Button
								{
									Text = "Set Neither",
									AutomationId = "SetNeitherButton"
								}.Assign(out var setNeitherButton),
								new Button
								{
									Text = "Reset",
									AutomationId = "ResetButton"
								}.Assign(out var resetButton),
							}
						},
						_orientationLabel,
						_offsetLabel,
						_lastActionLabel
					}
				}.Row(1),
				new Border
				{
					StrokeThickness = 1,
					Stroke = Color.FromArgb("#A0A0A0"),
					Padding = 10,
					Background = Color.FromArgb("#F7F7F7"),
					Content = new Label
					{
						Text = "Expected: changing to Neither should preserve the current scroll position. Actual bug: the offset snaps back to the origin."
					}
				}.Row(2),
				new Border
				{
					StrokeThickness = 1,
					Stroke = Color.FromArgb("#202020"),
					Background = Color.FromArgb("#FFFFFF"),
					HeightRequest = 360,
					Content = _bugScrollView
				}.Row(3)
			}
		};

		scrollToReproOffsetButton.Clicked += OnScrollToSampleClicked;
		setBothButton.Clicked += OnSetBothClicked;
		setNeitherButton.Clicked += OnSetNeitherClicked;
		resetButton.Clicked += OnResetClicked;

		RefreshState("Launch the app, tap 'Scroll To Repro Offset', then tap 'Set Neither'.");
	}

	Grid CreateScrollContent()
	{
		return new Grid
		{
			WidthRequest = 1400,
			HeightRequest = 1100,
			Background = Color.FromArgb("#FFF8E7"),
			RowDefinitions =
			{
				new RowDefinition(220),
				new RowDefinition(220),
				new RowDefinition(220),
				new RowDefinition(220),
				new RowDefinition(220)
			},
			ColumnDefinitions =
			{
				new ColumnDefinition(280),
				new ColumnDefinition(280),
				new ColumnDefinition(280),
				new ColumnDefinition(280),
				new ColumnDefinition(280)
			},
			Children =
			{
				new BoxView { Margin = 16, Color = Color.FromArgb("#F94144"), Opacity = 0.35 }.Row(0).Column(0),
				new BoxView { Margin = 16, Color = Color.FromArgb("#F3722C"), Opacity = 0.35 }.Row(0).Column(4),
				new BoxView { Margin = 16, Color = Color.FromArgb("#90BE6D"), Opacity = 0.45 }.Row(2).Column(2),
				new BoxView { Margin = 16, Color = Color.FromArgb("#577590"), Opacity = 0.35 }.Row(4).Column(0),
				new BoxView { Margin = 16, Color = Color.FromArgb("#277DA1"), Opacity = 0.35 }.Row(4).Column(4),
				new Border
				{
					Margin = 20,
					Padding = 12,
					Background = Color.FromArgb("#FFFFFF"),
					Stroke = Color.FromArgb("#222222"),
					Content = new Label
					{
						Text = "Origin (0,0)",
						FontAttributes = FontAttributes.Bold
					}
				}.Row(0).Column(0),
				new Border
				{
					Margin = 20,
					Padding = 12,
					Background = Color.FromArgb("#FFFFFF"),
					Stroke = Color.FromArgb("#222222"),
					Content = new VerticalStackLayout
					{
						Spacing = 6,
						Children =
						{
							new Label
							{
								Text = "Target zone",
								FontAttributes = FontAttributes.Bold
							},
							new Label
							{
								Text = "Tap 'Scroll To Repro Offset', then 'Set Neither'."
							}
						}
					}
				}.Row(2).Column(2),
				new Border
				{
					Margin = 20,
					Padding = 12,
					Background = Color.FromArgb("#FFFFFF"),
					Stroke = Color.FromArgb("#222222"),
					Content = new Label
					{
						Text = "Bottom-right marker",
						FontAttributes = FontAttributes.Bold
					}
				}.Row(4).Column(4)
			}
		};
	}

	async void OnScrollToSampleClicked(object sender, EventArgs e)
	{
		var orientationChanged = _bugScrollView.Orientation != ScrollOrientation.Both;
		_bugScrollView.Orientation = ScrollOrientation.Both;

		if (orientationChanged)
		{
			await Task.Delay(100);
		}

		await _bugScrollView.ScrollToAsync(ReproScrollX, ReproScrollY, false);
		await Task.Delay(50);
		RefreshState($"Scrolled to approx. X={ReproScrollX:0}, Y={ReproScrollY:0}.");
	}

	void OnSetBothClicked(object sender, EventArgs e)
	{
		_bugScrollView.Orientation = ScrollOrientation.Both;
		RefreshState("Orientation set to Both.");
	}

	async void OnSetNeitherClicked(object sender, EventArgs e)
	{
		var beforeX = _bugScrollView.ScrollX;
		var beforeY = _bugScrollView.ScrollY;

		_bugScrollView.Orientation = ScrollOrientation.Neither;

		await Task.Delay(100);
		RefreshState($"Set Orientation to Neither. Before: X={beforeX:0.##}, Y={beforeY:0.##}. After: X={_bugScrollView.ScrollX:0.##}, Y={_bugScrollView.ScrollY:0.##}.");
	}

	async void OnResetClicked(object sender, EventArgs e)
	{
		_bugScrollView.Orientation = ScrollOrientation.Both;
		await _bugScrollView.ScrollToAsync(0, 0, false);
		RefreshState("Reset orientation to Both and scrolled back to the origin.");
	}

	void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
	{
		RefreshState(_lastAction);
	}

	void RefreshState(string action)
	{
		_lastAction = action ?? string.Empty;
		_orientationLabel.Text = $"Orientation: {_bugScrollView.Orientation}";
		_offsetLabel.Text = $"ScrollX: {_bugScrollView.ScrollX.ToString("0.##", CultureInfo.InvariantCulture)} | ScrollY: {_bugScrollView.ScrollY.ToString("0.##", CultureInfo.InvariantCulture)}";
		_lastActionLabel.Text = $"Action: {_lastAction}";
	}
}

file static class Issue34671ViewExtensions
{
	public static TView Row<TView>(this TView view, int row)
		where TView : View
	{
		Grid.SetRow(view, row);
		return view;
	}

	public static TView Column<TView>(this TView view, int column)
		where TView : View
	{
		Grid.SetColumn(view, column);
		return view;
	}

	public static TView Assign<TView>(this TView view, out TView assigned)
		where TView : View
	{
		assigned = view;
		return view;
	}
}