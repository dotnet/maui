namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	int _selectionCount;
	int _showRequestCount;
	int _disabledCallbacks;
	readonly FlyoutBase _originalFlyout;
	bool _delayedTestRunning;
	int _longMenuSelections;

	public MainPage()
	{
		InitializeComponent();
		_originalFlyout = FlyoutBase.GetContextFlyout(label);
		DelayedTestButton.IsEnabled = OperatingSystem.IsIOSVersionAtLeast(16) ||
			OperatingSystem.IsMacCatalystVersionAtLeast(16);
		ShowMenuButton.IsEnabled = OperatingSystem.IsAndroid() || OperatingSystem.IsWindows() || OperatingSystem.IsIOSVersionAtLeast(16) ||
			OperatingSystem.IsMacCatalystVersionAtLeast(16);
		ShowLongMenuButton.IsEnabled = ShowMenuButton.IsEnabled;
		InitializeLongMenu();
		SupportLabel.Text = OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsMacCatalystVersionAtLeast(16)
			? "Show opens the framework's custom vertical popover. Light/Dark have arrows and submenu pages. This sample adds no gesture triggers."
			: ShowMenuButton.IsEnabled
				? "Show opens a native PopupMenu on Android or the attached WinUI flyout on Windows. This sample adds no gesture triggers."
				: "Programmatic opening requires Android, Windows, iOS 16+ or Mac Catalyst 16+.";

		Console.WriteLine("SANDBOX: Source context-menu sample loaded");
		System.Diagnostics.Debug.WriteLine($"[ContextFlyout] Sandbox loaded: Android={OperatingSystem.IsAndroid()}, Windows={OperatingSystem.IsWindows()}, iOS16+={OperatingSystem.IsIOSVersionAtLeast(16)}, MacCatalyst16+={OperatingSystem.IsMacCatalystVersionAtLeast(16)}, showButtonEnabled={ShowMenuButton.IsEnabled}.");
	}

	void OnShowContextMenuClicked(object? sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Programmatic context-menu requested");
		System.Diagnostics.Debug.WriteLine("[ContextFlyout] Sandbox Show button clicked.");
		ShowColorMenu();
	}

	void ShowColorMenu()
	{
		_showRequestCount++;
		UpdateActionCounts();
		FlyoutBase.ShowContextFlyout(label);
	}

	void InitializeLongMenu()
	{
		var flyout = new MenuFlyout();
		var submenu = new MenuFlyoutSubItem { Text = "More items (25 nested actions)" };
		for (int i = 1; i <= 25; i++)
		{
			submenu.Add(CreateLongMenuItem($"Nested item {i:D2}"));
			flyout.Add(CreateLongMenuItem($"Root item {i:D2}"));
		}
		flyout.Insert(0, submenu);
		flyout.Insert(1, new MenuFlyoutSeparator());
		flyout.Add(CreateLongMenuItem("Disabled last action", enabled: false));
		FlyoutBase.SetContextFlyout(LongMenuTarget, flyout);
	}

	MenuFlyoutItem CreateLongMenuItem(string text, bool enabled = true)
	{
		var item = new MenuFlyoutItem { Text = text, IsEnabled = enabled };
		item.Clicked += OnLongMenuItemSelected;
		return item;
	}

	void OnShowLongMenuClicked(object? sender, EventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("[ContextFlyout] Sandbox scrollable menu requested.");
		FlyoutBase.ShowContextFlyout(LongMenuTarget);
	}

	void OnLongMenuItemSelected(object? sender, EventArgs e)
	{
		if (sender is not MenuFlyoutItem item)
			throw new InvalidOperationException("The scrollable menu selection must be a menu item.");

		LongMenuStatusLabel.Text = $"Selected {item.Text}; total callbacks: {++_longMenuSelections}.";
		if (!item.IsEnabled)
			LongMenuStatusLabel.Text += " FAIL: disabled action callback.";
		System.Diagnostics.Debug.WriteLine($"[ContextFlyout] Sandbox scrollable menu: {LongMenuStatusLabel.Text}");
	}

	void OnResetLongMenuClicked(object? sender, EventArgs e)
	{
		_longMenuSelections = 0;
		LongMenuStatusLabel.Text = "No selection yet.";
	}

	void OnColorSelected(object? sender, EventArgs e)
	{
		if (sender is not MenuFlyoutItem { CommandParameter: string color })
			throw new InvalidOperationException("A color menu item must provide a color parameter.");

		if (!((MenuFlyoutItem)sender).IsEnabled)
			_disabledCallbacks++;
		SampleRoot.BackgroundColor = Color.Parse(color);
		StatusLabel.Text = $"Selected {color} ({++_selectionCount})";
		Console.WriteLine($"SANDBOX: {StatusLabel.Text}");
		System.Diagnostics.Debug.WriteLine($"[ContextFlyout] Sandbox selection: {StatusLabel.Text}.");
		UpdateActionCounts();
	}

	void OnResetClicked(object? sender, EventArgs e)
	{
		_selectionCount = 0;
		_showRequestCount = 0;
		_disabledCallbacks = 0;
		UpdateActionCounts();
		SampleRoot.BackgroundColor = Colors.Yellow;
		StatusLabel.Text = "Ready";
		Console.WriteLine("SANDBOX: Color and selection count reset");
	}

	void UpdateActionCounts()
	{
		ActionCountsLabel.Text = $"Show requests: {_showRequestCount}; actions from all triggers: {_selectionCount}; disabled callbacks: {_disabledCallbacks}. Compare action counts before and after each selection.";
		if (_disabledCallbacks > 0)
			ActionCountsLabel.Text += " FAIL: disabled action callback.";
		System.Diagnostics.Debug.WriteLine($"[ContextFlyout] Sandbox counters: {ActionCountsLabel.Text}");
	}

	void OnCaptureDiagnosticsClicked(object? sender, EventArgs e) => CaptureDiagnostics();

	void CaptureDiagnostics()
	{
		string native = "Apple custom presenter diagnostics are unavailable on this platform.";
#if IOS || MACCATALYST
		if (OperatingSystem.IsIOSVersionAtLeast(16) || OperatingSystem.IsMacCatalystVersionAtLeast(16))
		{
			var target = label.Handler?.PlatformView as UIKit.UIView;
			native = $"VoiceOver running: {UIKit.UIAccessibility.IsVoiceOverRunning}; " +
				$"content size category: {target?.TraitCollection.PreferredContentSizeCategory}; " +
				$"window attached: {target?.Window is not null}; " +
				$"native bounds: {target?.Bounds}.";
		}
#endif
		DiagnosticsLabel.Text = $"Page: {Width:F0} x {Height:F0}; target: {label.Width:F0} x {label.Height:F0}; handler: {label.Handler?.GetType().Name ?? "none"}. {native}";
		System.Diagnostics.Debug.WriteLine($"[ContextFlyout] Sandbox snapshot: {DiagnosticsLabel.Text}");
	}

	async void OnDelayedTestClicked(object? sender, EventArgs e)
	{
		if (_delayedTestRunning)
			return;

		_delayedTestRunning = true;
		DelayedTestButton.IsEnabled = false;
		var scenario = CleanupScenarioPicker.SelectedIndex;
		var actionsBefore = _selectionCount;
		var nativeTarget = label.Handler?.PlatformView;
		if (!label.IsLoaded || nativeTarget is null || FlyoutBase.GetContextFlyout(label) is null)
		{
			CleanupResultLabel.Text = "Restore the target and menu before running another test.";
			_delayedTestRunning = false;
			DelayedTestButton.IsEnabled = true;
			return;
		}
		CleanupResultLabel.Text = "Opening menu. In 5 seconds the selected operation will run; do not select an action.";
		try
		{
			ShowColorMenu();
			await Task.Delay(TimeSpan.FromSeconds(5));

			// Navigation away cancels this page's delayed operation.
			if (!IsLoaded)
			{
				System.Diagnostics.Debug.WriteLine("[ContextFlyout] Sandbox delayed test cancelled: page is unloaded.");
				return;
			}

			if (scenario == 3)
			{
				CaptureDiagnostics();
				CleanupResultLabel.Text = "Snapshot captured with menu left open. Check rotation, text wrapping, accessibility and focus manually.";
				return;
			}
			else if (scenario == 0)
				FlyoutBase.SetContextFlyout(label, null!);
			else if (scenario == 1)
				SampleRoot.Children.Remove(label);
			else if (scenario == 2)
			{
				label.Handler?.DisconnectHandler();
				label.Handler = null;
			}

			await Task.Delay(250);
			string cleanup = "Inspect native dismissal manually.";
			var actionDelta = _selectionCount - actionsBefore;
			CleanupResultLabel.Text = $"{cleanup} {(actionDelta == 0 ? "PASS" : "FAIL")}: actions during test: {actionDelta} (expected 0). Confirm the menu disappeared and check focus manually.";
			System.Diagnostics.Debug.WriteLine($"[ContextFlyout] Sandbox cleanup: {CleanupResultLabel.Text}");
			CaptureDiagnostics();
		}
		finally
		{
			_delayedTestRunning = false;
			DelayedTestButton.IsEnabled = true;
		}
	}

	void OnRestoreTargetClicked(object? sender, EventArgs e)
	{
		if (_delayedTestRunning)
		{
			CleanupResultLabel.Text = "Wait for the delayed test to finish before restoring.";
			return;
		}

		FlyoutBase.SetContextFlyout(label, _originalFlyout);
		if (SampleRoot.Children.Contains(label))
			SampleRoot.Children.Remove(label);
		SampleRoot.Children.Add(label);
		CleanupResultLabel.Text = "Target restored. Reopen using Show; verify exactly one callback.";
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		System.Diagnostics.Debug.WriteLine($"[ContextFlyout] Sandbox layout: {width:F0} x {height:F0}.");
	}
}