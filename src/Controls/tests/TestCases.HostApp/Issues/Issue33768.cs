#nullable enable
namespace Maui.Controls.Sample.Issues;

/// <summary>
/// Test for Issue #33768 - Performance degradation caused by infinite RequestApplyInsets loop.
/// 
/// This is a regression test to verify that pages with large scrollable content do not
/// trigger excessive GC activity. Issue #33768 was closed as a duplicate of #33731,
/// as both had the same root cause: PR #33285's viewExtendsBeyondScreen check causing
/// infinite RequestApplyInsets loops.
/// 
/// This test uses a ScrollView with many items to verify that normal scrollable content
/// doesn't cause the infinite loop. While the TabbedPage scenario (#33731) is the primary
/// reproduction case (ViewPager positions inactive tabs off-screen), this test ensures
/// that general scrollable content also works correctly.
/// 
/// WITH BUG: Potential for excessive GC due to views positioned beyond screen bounds
/// WITH FIX: No excessive GC activity for normal scrollable content
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

		// Create a ScrollView with a very tall VerticalStackLayout
		// The native VerticalStackLayout view will have bounds extending beyond screenHeight
		// This is the exact scenario described in Issue #33768
		var scrollContent = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 10
		};

		// Add many items to make the content extend well beyond screen height
		// The issue mentions: "for Scrollable Content (like ScrollView, CollectionView, 
		// or VerticalStackLayout inside a ScrollView), extending beyond screenHeight 
		// is a valid and permanent state"
		for (int i = 0; i < 100; i++)
		{
			scrollContent.Children.Add(new Border
			{
				Padding = 10,
				BackgroundColor = i % 2 == 0 ? Colors.LightGray : Colors.White,
				StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 5 },
				Content = new Label
				{
					Text = $"Item {i + 1} - This content extends the VerticalStackLayout beyond screen bounds",
					FontSize = 14
				}
			});
		}

		var scrollView = new ScrollView
		{
			AutomationId = "TestScrollView",
			Content = scrollContent
		};

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
				new VerticalStackLayout
				{
					Padding = 20,
					Spacing = 10,
					BackgroundColor = Colors.LightBlue,
					Children =
					{
						new Label
						{
							Text = "Issue #33768 - ScrollView Content Loop Test",
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
							Text = "ScrollView contains 100 items.\n" +
							       "WITH BUG: GCCount increases rapidly\n" +
							       "WITH FIX: GCCount stays at 0-1",
							FontSize = 12,
							HorizontalOptions = LayoutOptions.Center
						}
					}
				}.Apply(v => Grid.SetRow((BindableObject)v, 0)),

				// ScrollView in the middle
				scrollView.Apply(v => Grid.SetRow((BindableObject)v, 1)),

				// Footer with verdict
				new VerticalStackLayout
				{
					Padding = 10,
					BackgroundColor = Colors.LightGreen,
					Children =
					{
						new Label { Text = "Verdict:", FontAttributes = FontAttributes.Bold },
						_verdictLabel
					}
				}.Apply(v => Grid.SetRow((BindableObject)v, 2))
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
				_statusLabel.Text = "BUG DETECTED: Infinite loop likely occurring";
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
}

// Extension method to allow fluent Grid.SetRow in object initializers
file static class ViewExtensions
{
	public static T Apply<T>(this T view, Action<T> action)
	{
		action(view);
		return view;
	}
}
