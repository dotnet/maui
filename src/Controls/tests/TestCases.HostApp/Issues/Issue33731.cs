#nullable enable
namespace Maui.Controls.Sample.Issues;

/// <summary>
/// Test for Issue #33731 - Continuous GC logs on TabbedPage in MAUI 10.0.30.
/// 
/// The bug causes an infinite loop of RequestApplyInsets calls when Tab 2's content
/// is positioned off-screen (at x=screenWidth). This creates continuous lambda
/// allocations (~60/sec), triggering GC every ~5-6 seconds.
/// 
/// This test monitors GC.CollectionCount(0) to detect excessive GC activity.
/// WITH BUG: 5+ GC events in 30 seconds
/// WITH FIX: 0-1 GC events in 30 seconds
/// </summary>
[Issue(IssueTracker.Github, 33731, "Continuous GC logs on TabbedPage in MAUI 10.0.30", PlatformAffected.Android)]
public class Issue33731 : TabbedPage
{
	private readonly Label _gcCountLabel;
	private readonly Label _statusLabel;
	private int _lastGcCount;
	private int _gcEventsDetected;
	private DateTime _startTime;
	private IDispatcherTimer? _timer;

	public Issue33731()
	{
		AutomationId = "TabbedPageRoot";

		// Force initial GC to establish baseline
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		_lastGcCount = GC.CollectionCount(0);
		_gcEventsDetected = 0;
		_startTime = DateTime.UtcNow;

		// Create the monitoring UI on Tab 1
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

		var tab1 = new ContentPage
		{
			Title = "GC Monitor",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label 
					{ 
						Text = "Issue #33731 - TabbedPage GC Monitor", 
						AutomationId = "Tab1Label",
						FontSize = 18, 
						FontAttributes = FontAttributes.Bold, 
						HorizontalOptions = LayoutOptions.Center 
					},
					new BoxView { HeightRequest = 2, Color = Colors.Gray },
					_gcCountLabel,
					_statusLabel,
					new Label
					{
						Text = "WITH BUG: GCCount increases rapidly (5+ in 30s)\n" +
						       "WITH FIX: GCCount stays at 0-1",
						FontSize = 12,
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(0, 20, 0, 0)
					}
				}
			}
		};

		// Create second tab with Grid (matching the exact repro from the issue)
		var tab2 = new ContentPage
		{
			Title = "Tab 2",
			Content = new Grid
			{
				Children =
				{
					new Label
					{
						Text = "Tab 2 - Inactive tab",
						AutomationId = "Tab2Label",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 24
					}
				}
			}
		};

		// Create third tab (more tabs = higher likelihood of bug triggering)
		var tab3 = new ContentPage
		{
			Title = "Tab 3",
			Content = new Grid
			{
				Children =
				{
					new Label
					{
						Text = "Tab 3 - Another inactive tab",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 24
					}
				}
			}
		};

		Children.Add(tab1);
		Children.Add(tab2);
		Children.Add(tab3);

		// Start GC monitoring timer
		_timer = Application.Current?.Dispatcher.CreateTimer();
		if (_timer != null)
		{
			_timer.Interval = TimeSpan.FromMilliseconds(500);
			_timer.Tick += OnTimerTick;
			_timer.Start();
		}
	}

	private void OnTimerTick(object? sender, EventArgs e)
	{
		int currentGcCount = GC.CollectionCount(0);

		if (currentGcCount > _lastGcCount)
		{
			int newGCs = currentGcCount - _lastGcCount;
			_gcEventsDetected += newGCs;
			_lastGcCount = currentGcCount;
		}

		// Update UI - format must match what the test expects: "GCCount: X"
		_gcCountLabel.Text = $"GCCount: {_gcEventsDetected}";

		// Calculate elapsed time
		double elapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;

		// Update status based on results
		if (elapsedSeconds >= 30)
		{
			if (_gcEventsDetected >= 5)
			{
				_statusLabel.Text = $"BUG DETECTED: {_gcEventsDetected} GCs in {elapsedSeconds:F0}s";
			}
			else if (_gcEventsDetected <= 1)
			{
				_statusLabel.Text = $"PASS: Only {_gcEventsDetected} GCs in {elapsedSeconds:F0}s";
			}
			else
			{
				_statusLabel.Text = $"BORDERLINE: {_gcEventsDetected} GCs in {elapsedSeconds:F0}s";
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
}
