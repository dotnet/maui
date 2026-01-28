namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33731, "Continuous GC logs on TabbedPage in MAUI 10.0.30", PlatformAffected.Android)]
public class Issue33731 : TabbedPage
{
	private Label _insetCountLabel;
	private IDispatcherTimer _updateTimer;
	private int _layoutPassCount = 0;

	public Issue33731()
	{
		AutomationId = "TabbedPageRoot";

		// Label to show layout pass count (updated via timer)
		_insetCountLabel = new Label
		{
			Text = "LayoutCount: 0",
			AutomationId = "LayoutCountLabel",
			BackgroundColor = Colors.Yellow,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			Padding = new Thickness(10),
			FontSize = 18
		};

		// Create first tab with Grid (matching the exact repro from the issue)
		var tab1Content = new Grid
		{
			Children =
			{
				new VerticalStackLayout
				{
					Spacing = 10,
					Children =
					{
						_insetCountLabel,
						new Label
						{
							Text = "Tab 1 - Watch LayoutCount above",
							AutomationId = "Tab1Label",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
							FontSize = 20
						},
						new Label
						{
							Text = "Bug: Count keeps increasing (infinite loop)",
							HorizontalOptions = LayoutOptions.Center,
							FontSize = 14,
							TextColor = Colors.Red
						}
					}
				}
			}
		};

		var tab1 = new ContentPage
		{
			Title = "Tab 1",
			Content = tab1Content
		};

		// Create second tab with Grid (matching the exact repro from the issue)
		var tab2Content = new Grid
		{
			Children =
			{
				new Label
				{
					Text = "Tab 2",
					AutomationId = "Tab2Label",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 24
				}
			}
		};

		var tab2 = new ContentPage
		{
			Title = "Tab 2",
			Content = tab2Content
		};

		// Hook into layout events on the content to detect excessive layouts
		// The bug causes Tab 2's content to continuously trigger layout due to inset thrashing
		tab2Content.SizeChanged += (s, e) => 
		{
			_layoutPassCount++;
			System.Diagnostics.Debug.WriteLine($"[Issue33731] Tab2 SizeChanged #{_layoutPassCount}");
		};

#if ANDROID
		// Monitor the platform view's layout requests
		tab2.HandlerChanged += (s, e) =>
		{
			if (tab2.Handler?.PlatformView is Android.Views.View platformView)
			{
				platformView.ViewTreeObserver.GlobalLayout += (sender, args) =>
				{
					_layoutPassCount++;
				};
			}
		};
#endif

		// Add tabs to TabbedPage
		Children.Add(tab1);
		Children.Add(tab2);

		// Start a timer to update the layout count display
		_updateTimer = Application.Current.Dispatcher.CreateTimer();
		_updateTimer.Interval = TimeSpan.FromMilliseconds(100);
		_updateTimer.Tick += OnTimerTick;
		_updateTimer.Start();
	}

	private void OnTimerTick(object sender, EventArgs e)
	{
		_insetCountLabel.Text = $"LayoutCount: {_layoutPassCount}";
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_updateTimer?.Stop();
	}
}
