using System.Diagnostics;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	private System.Timers.Timer? _monitorTimer;
	private int _initialGCCount;
	private long _initialAllocatedBytes;
	private DateTime _testStartTime;
	private int _lastGCCount;
	private DateTime _lastGCTime;
	private List<double> _gcRateSamples = new();
	private List<double> _allocationRateSamples = new();

	public MainPage()
	{
		InitializeComponent();
		Console.WriteLine("SANDBOX: MainPage initialized");
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Console.WriteLine("SANDBOX: MainPage appeared");
	}

	private void OnStartTestClicked(object? sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Start Test button clicked");
		StartButton.IsEnabled = false;
		StartMonitoring();
	}

	private async void OnViewTabbedPageClicked(object? sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: View TabbedPage button clicked");
		
		// Create and navigate to TabbedPage
		var tabbedPage = new Issue33731TabbedPage();
		await Navigation.PushAsync(tabbedPage);
		
		Console.WriteLine("SANDBOX: Navigated to TabbedPage");
	}

	private void StartMonitoring()
	{
		Console.WriteLine("SANDBOX: Starting GC monitoring");
		
		// Force a GC to establish baseline
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		
		// Record initial state
		_initialGCCount = GC.CollectionCount(0);
		_initialAllocatedBytes = GC.GetTotalAllocatedBytes(precise: true);
		_testStartTime = DateTime.UtcNow;
		_lastGCCount = _initialGCCount;
		_lastGCTime = _testStartTime;
		_gcRateSamples.Clear();
		_allocationRateSamples.Clear();
		
		Console.WriteLine($"SANDBOX: Baseline - GC Count: {_initialGCCount}, Allocated: {_initialAllocatedBytes / (1024.0 * 1024.0):F2} MB");
		
		StatusLabel.Text = "Monitoring... (15 seconds)";
		
		// Monitor every 500ms for 15 seconds
		int updateCount = 0;
		_monitorTimer = new System.Timers.Timer(500);
		_monitorTimer.Elapsed += (s, e) =>
		{
			updateCount++;
			UpdateMetrics();
			
			// Stop after 15 seconds (30 updates)
			if (updateCount >= 30)
			{
				_monitorTimer?.Stop();
				_monitorTimer?.Dispose();
				_monitorTimer = null;
				
				MainThread.BeginInvokeOnMainThread(() =>
				{
					AnalyzeResults();
				});
			}
		};
		_monitorTimer.Start();
	}

	private void UpdateMetrics()
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			try
			{
				// Get current metrics
				int currentGCCount = GC.CollectionCount(0);
				long currentAllocatedBytes = GC.GetTotalAllocatedBytes(precise: true);
				DateTime now = DateTime.UtcNow;
				
				// Calculate totals from start
				int totalGCsSinceStart = currentGCCount - _initialGCCount;
				double totalSeconds = (now - _testStartTime).TotalSeconds;
				long totalAllocated = currentAllocatedBytes - _initialAllocatedBytes;
				
				// Calculate rates
				double gcRatePerSecond = totalGCsSinceStart / Math.Max(totalSeconds, 0.1);
				double allocationRateMBPerSec = (totalAllocated / (1024.0 * 1024.0)) / Math.Max(totalSeconds, 0.1);
				
				// Track instantaneous GC rate
				double timeSinceLastCheck = (now - _lastGCTime).TotalSeconds;
				int gcsSinceLastCheck = currentGCCount - _lastGCCount;
				if (timeSinceLastCheck > 0)
				{
					double instantGCRate = gcsSinceLastCheck / timeSinceLastCheck;
					_gcRateSamples.Add(instantGCRate);
					_allocationRateSamples.Add(allocationRateMBPerSec);
				}
				
				_lastGCCount = currentGCCount;
				_lastGCTime = now;
				
				// Update UI
				GCCountLabel.Text = totalGCsSinceStart.ToString();
				GCRateLabel.Text = gcRatePerSecond.ToString("F2");
				TotalAllocatedLabel.Text = (totalAllocated / (1024.0 * 1024.0)).ToString("F2");
				AllocationRateLabel.Text = allocationRateMBPerSec.ToString("F2");
				
				// Log periodically
				if (totalGCsSinceStart > 0)
				{
					Console.WriteLine($"SANDBOX: GCs={totalGCsSinceStart}, Rate={gcRatePerSecond:F2}/sec, Allocated={totalAllocated / (1024.0 * 1024.0):F2}MB, AllocRate={allocationRateMBPerSec:F2}MB/sec");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SANDBOX ERROR: {ex.Message}");
			}
		});
	}

	private void AnalyzeResults()
	{
		Console.WriteLine("SANDBOX: Analyzing results...");
		
		int totalGCs = GC.CollectionCount(0) - _initialGCCount;
		double totalSeconds = (DateTime.UtcNow - _testStartTime).TotalSeconds;
		double avgGCRate = totalGCs / totalSeconds;
		
		// Calculate average allocation rate from samples
		double avgAllocationRate = _allocationRateSamples.Count > 0 
			? _allocationRateSamples.Average() 
			: 0;
		
		// Calculate max instantaneous GC rate
		double maxGCRate = _gcRateSamples.Count > 0 
			? _gcRateSamples.Max() 
			: 0;
		
		Console.WriteLine($"SANDBOX: Total GCs: {totalGCs}");
		Console.WriteLine($"SANDBOX: Avg GC Rate: {avgGCRate:F2}/sec");
		Console.WriteLine($"SANDBOX: Max GC Rate: {maxGCRate:F2}/sec");
		Console.WriteLine($"SANDBOX: Avg Allocation Rate: {avgAllocationRate:F2} MB/sec");
		
		// Determine verdict
		// With the bug: 
		// - Lambda allocations ~60 times/sec cause ~2-3 GCs in 15 seconds
		// - GC rate typically 0.15-0.30 per second
		// - Allocation rate typically > 0.5 MB/sec
		//
		// Without the bug:
		// - Very few or no GCs (0-1)
		// - GC rate < 0.1 per second
		// - Much lower allocation rate
		
		string verdict;
		string status;
		
		if (totalGCs >= 2 && avgGCRate >= 0.12)
		{
			verdict = "🔴 BUG DETECTED - Excessive GC activity!";
			status = $"Bug present: {totalGCs} GCs in {totalSeconds:F1}s (rate: {avgGCRate:F2}/sec)";
			Console.WriteLine("SANDBOX VERDICT: BUG DETECTED");
		}
		else if (totalGCs <= 1 && avgGCRate < 0.1)
		{
			verdict = "✅ PASS - Normal GC behavior";
			status = $"Healthy: Only {totalGCs} GCs in {totalSeconds:F1}s (rate: {avgGCRate:F2}/sec)";
			Console.WriteLine("SANDBOX VERDICT: PASS");
		}
		else
		{
			verdict = "⚠️ UNCLEAR - Borderline behavior";
			status = $"Borderline: {totalGCs} GCs in {totalSeconds:F1}s (rate: {avgGCRate:F2}/sec)";
			Console.WriteLine("SANDBOX VERDICT: UNCLEAR");
		}
		
		StatusLabel.Text = status;
		VerdictLabel.Text = verdict;
		
		Console.WriteLine($"SANDBOX: Test complete - {verdict}");
		StartButton.IsEnabled = true;
	}
}

// Test TabbedPage matching the issue reproduction with built-in GC monitoring
public class Issue33731TabbedPage : TabbedPage
{
	private readonly Label _gcCountLabel;
	private readonly Label _statusLabel;
	private int _lastGcCount;
	private int _gcEventsDetected;
	private DateTime _startTime;
	private IDispatcherTimer? _timer;

	public Issue33731TabbedPage()
	{
		Console.WriteLine("SANDBOX: Creating TabbedPage with GC monitoring");
		
		// Create the monitoring UI on Tab 1
		_gcCountLabel = new Label 
		{ 
			AutomationId = "GCCountLabel", 
			Text = "GC Events: 0",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center
		};
		
		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Monitoring...",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 20, 0, 0)
		};
		
		var instructionLabel = new Label
		{
			Text = "This TabbedPage monitors GC activity.\n\n" +
			       "WITH BUG: GC Events will rapidly increase (5+ in 30s)\n" +
			       "WITHOUT BUG: GC Events stay at 0-1\n\n" +
			       "The bug causes RequestApplyInsets to be called ~60x/sec on inactive tabs.",
			FontSize = 12,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			Margin = new Thickness(20)
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
					new Label { Text = "Issue #33731 - TabbedPage GC Monitor", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },
					new BoxView { HeightRequest = 2, Color = Colors.Gray, Margin = new Thickness(0, 10) },
					_gcCountLabel,
					_statusLabel,
					new BoxView { HeightRequest = 1, Color = Colors.LightGray, Margin = new Thickness(0, 20) },
					instructionLabel
				}
			}
		};
		
		var tab2 = new ContentPage 
		{ 
			Title = "Tab 2", 
			Content = new Grid 
			{ 
				Children = 
				{ 
					new Label 
					{ 
						Text = "Tab 2 Content\n\nThis tab is inactive when on Tab 1.\nThe bug causes excessive allocations for off-screen views.",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(20)
					} 
				} 
			} 
		};
		
		var tab3 = new ContentPage 
		{ 
			Title = "Tab 3", 
			Content = new Grid 
			{ 
				Children = 
				{ 
					new Label 
					{ 
						Text = "Tab 3 Content\n\nAnother inactive tab to increase the repro likelihood.",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(20)
					} 
				} 
			} 
		};
		
		Children.Add(tab1);
		Children.Add(tab2);
		Children.Add(tab3);
		
		Console.WriteLine("SANDBOX: TabbedPage created with 3 tabs");
		
		// Start GC monitoring
		StartGCMonitoring();
	}
	
	private void StartGCMonitoring()
	{
		Console.WriteLine("SANDBOX: Starting GC monitoring on TabbedPage");
		
		// Force initial GC to establish baseline
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		
		_lastGcCount = GC.CollectionCount(0);
		_gcEventsDetected = 0;
		_startTime = DateTime.UtcNow;
		
		Console.WriteLine($"SANDBOX: Initial GC Count (Gen 0): {_lastGcCount}");
		
		// Create timer to monitor GC every 500ms
		_timer = Application.Current?.Dispatcher.CreateTimer();
		if (_timer != null)
		{
			_timer.Interval = TimeSpan.FromMilliseconds(500);
			_timer.Tick += OnTimerTick;
			_timer.Start();
			Console.WriteLine("SANDBOX: GC monitoring timer started");
		}
		else
		{
			Console.WriteLine("SANDBOX ERROR: Could not create timer");
		}
	}
	
	private void OnTimerTick(object? sender, EventArgs e)
	{
		try
		{
			int currentGcCount = GC.CollectionCount(0);
			
			if (currentGcCount > _lastGcCount)
			{
				int newGCs = currentGcCount - _lastGcCount;
				_gcEventsDetected += newGCs;
				_lastGcCount = currentGcCount;
				
				Console.WriteLine($"SANDBOX: GC detected! Total GC Events: {_gcEventsDetected}");
			}
			
			// Update UI
			_gcCountLabel.Text = $"GC Events: {_gcEventsDetected}";
			
			// Calculate elapsed time
			double elapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;
			
			// Update status based on results
			if (elapsedSeconds >= 30)
			{
				// After 30 seconds, provide verdict
				if (_gcEventsDetected >= 5)
				{
					_statusLabel.Text = $"🔴 BUG DETECTED after {elapsedSeconds:F0}s\n{_gcEventsDetected} GCs indicate excessive allocations";
					Console.WriteLine($"SANDBOX VERDICT: BUG DETECTED - {_gcEventsDetected} GCs in {elapsedSeconds:F1}s");
				}
				else if (_gcEventsDetected <= 1)
				{
					_statusLabel.Text = $"✅ PASS after {elapsedSeconds:F0}s\n{_gcEventsDetected} GCs is normal behavior";
					Console.WriteLine($"SANDBOX VERDICT: PASS - Only {_gcEventsDetected} GCs in {elapsedSeconds:F1}s");
				}
				else
				{
					_statusLabel.Text = $"⚠️ BORDERLINE after {elapsedSeconds:F0}s\n{_gcEventsDetected} GCs is unclear";
					Console.WriteLine($"SANDBOX VERDICT: BORDERLINE - {_gcEventsDetected} GCs in {elapsedSeconds:F1}s");
				}
			}
			else
			{
				_statusLabel.Text = $"Monitoring... {elapsedSeconds:F0}s elapsed";
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"SANDBOX ERROR in GC monitoring: {ex.Message}");
			_statusLabel.Text = $"Error: {ex.Message}";
		}
	}
}