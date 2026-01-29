#nullable enable
namespace Maui.Controls.Sample.Issues;

/// <summary>
/// Test for Issue #33768 - Performance degradation caused by infinite RequestApplyInsets loop.
/// 
/// This test reproduces the bug by using a CollectionView with a NEGATIVE MARGIN (-50).
/// The negative margin causes the native view bounds to extend beyond the screen edges,
/// which triggers the viewExtendsBeyondScreen check in SafeAreaExtensions.cs.
/// 
/// WITHOUT FIX: The check triggers view.Post(() => RequestApplyInsets(view)), which
/// causes an infinite loop because the bounds never change, leading to ~60 allocations/sec
/// and triggering GC every 5-6 seconds.
/// 
/// WITH FIX: The IRequestInsetsOnTransition guard ensures only Shell fragments during
/// transitions get the re-apply behavior, preventing the infinite loop.
/// 
/// WITH BUG: 5+ GC events in 30 seconds
/// WITH FIX: 0-1 GC events in 30 seconds
/// </summary>
[Issue(IssueTracker.Github, 33768, "Performance degradation on Android caused by Infinite Layout Loop (RequestApplyInsets)", PlatformAffected.Android)]
public class Issue33768 : ContentPage
{
	private readonly Label _gcCountLabel;
	private readonly Label _statusLabel;
	private readonly Label _verdictLabel;
	private int _initialGcCount;
	private int _gcEventsDetected;
	private DateTime _startTime;
	private IDispatcherTimer? _timer;

	public Issue33768()
	{
		AutomationId = "Issue33768Root";

		// Force initial GC to establish baseline
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		_initialGcCount = GC.CollectionCount(0);
		_gcEventsDetected = 0;
		_startTime = DateTime.UtcNow;

		_gcCountLabel = new Label
		{
			AutomationId = "GCCountLabel",
			Text = "GCCount: 0",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			BackgroundColor = Colors.Yellow,
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(10)
		};

		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Monitoring...",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center
		};

		_verdictLabel = new Label
		{
			AutomationId = "VerdictLabel",
			Text = "Testing...",
			FontSize = 20,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center
		};

		// Create a CollectionView with NEGATIVE MARGIN
		// This is the key reproduction scenario from Issue #33768:
		// "Add a heavily populated CollectionView with a negative margin to force
		// some of the scrolling off-screen"
		// The negative margin causes native view bounds to extend beyond screen edges
		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			Margin = new Thickness(-50), // NEGATIVE MARGIN - triggers viewExtendsBeyondScreen
			BackgroundColor = Colors.LightBlue,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid { Padding = 10, HeightRequest = 80 };
				var border = new Border
				{
					Stroke = Colors.DarkBlue,
					StrokeThickness = 2,
					BackgroundColor = Colors.White,
					Padding = 10
				};
				var stack = new VerticalStackLayout();
				var titleLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 16 };
				titleLabel.SetBinding(Label.TextProperty, "Title");
				var descLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
				descLabel.SetBinding(Label.TextProperty, "Description");
				stack.Children.Add(titleLabel);
				stack.Children.Add(descLabel);
				border.Content = stack;
				grid.Children.Add(border);
				return grid;
			})
		};

		// Populate with 100 items
		var items = new List<CollectionItem>();
		for (int i = 1; i <= 100; i++)
		{
			items.Add(new CollectionItem
			{
				Title = $"Item {i}",
				Description = "CollectionView with Margin=-50 extends native bounds beyond screen"
			});
		}
		collectionView.ItemsSource = items;

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Auto }
			},
			Children =
			{
				// Header with monitoring UI
				CreateHeader(),
				// CollectionView with negative margin in the middle
				CreateMiddle(collectionView),
				// Footer with verdict
				CreateFooter()
			}
		};

		// Start GC monitoring timer
		_timer = Application.Current?.Dispatcher.CreateTimer();
		if (_timer != null)
		{
			_timer.Interval = TimeSpan.FromMilliseconds(500);
			_timer.Tick += OnTimerTick;
			_timer.Start();
		}
	}

	private View CreateHeader()
	{
		var header = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			BackgroundColor = Colors.LightBlue,
			Children =
			{
				new Label
				{
					Text = "Issue #33768 - Negative Margin GC Test",
					AutomationId = "TitleLabel",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center
				},
				new BoxView { HeightRequest = 2, Color = Colors.Gray },
				_gcCountLabel,
				_statusLabel,
				new Label
				{
					Text = "CollectionView has Margin=-50.\n" +
					       "WITH BUG: GCCount increases rapidly (5+ in 30s)\n" +
					       "WITH FIX: GCCount stays at 0-1",
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Center
				}
			}
		};
		Grid.SetRow(header, 0);
		return header;
	}

	private View CreateMiddle(CollectionView collectionView)
	{
		Grid.SetRow(collectionView, 1);
		return collectionView;
	}

	private View CreateFooter()
	{
		var footer = new VerticalStackLayout
		{
			Padding = 10,
			BackgroundColor = Colors.LightGreen,
			Children =
			{
				new Label { Text = "Verdict:", FontAttributes = FontAttributes.Bold },
				_verdictLabel
			}
		};
		Grid.SetRow(footer, 2);
		return footer;
	}

	private void OnTimerTick(object? sender, EventArgs e)
	{
		int currentGcCount = GC.CollectionCount(0);
		int newGCs = currentGcCount - _initialGcCount;

		if (newGCs > _gcEventsDetected)
		{
			_gcEventsDetected = newGCs;
		}

		// Update UI - format must match what the test expects: "GCCount: X"
		_gcCountLabel.Text = $"GCCount: {_gcEventsDetected}";

		// Calculate elapsed time
		double elapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;

		// Update status based on results
		if (elapsedSeconds >= 10)
		{
			if (_gcEventsDetected >= 3)
			{
				_verdictLabel.Text = $"FAIL: {_gcEventsDetected} GCs in {elapsedSeconds:F0}s";
				_verdictLabel.TextColor = Colors.Red;
				_statusLabel.Text = "BUG DETECTED: Infinite loop occurring";
			}
			else
			{
				_verdictLabel.Text = $"PASS: {_gcEventsDetected} GCs in {elapsedSeconds:F0}s";
				_verdictLabel.TextColor = Colors.Green;
				_statusLabel.Text = "No infinite loop detected";
			}
		}
		else
		{
			_statusLabel.Text = $"Monitoring... {elapsedSeconds:F0}s elapsed";
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_timer?.Stop();
	}

	// Simple data class for CollectionView items
	private class CollectionItem
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}
}
