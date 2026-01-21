using System;
using Microsoft.Maui.Controls;
#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14142, "[Windows] Window was already activated when minimize and restore from taskbar", PlatformAffected.UWP)]
public class Issue14142 : ContentPage
{
	private int _activated = 0;
	private int _deactivated = 0;
	private int _resumed = 0;
	private Label _statusLabel;
	private Label _resultLabel;
	private Window _window;

#if WINDOWS
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	private const int SW_MINIMIZE = 6;
	private const int SW_RESTORE = 9;
#endif

	public Issue14142()
	{
		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Activated: 0, Deactivated: 0, Resumed: 0",
			FontSize = 18,
			HorizontalOptions = LayoutOptions.Center
		};

		_resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			Text = "Waiting...",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center
		};

		var minimizeButton = new Button
		{
			AutomationId = "MinimizeButton",
			Text = "Minimize Window"
		};
		minimizeButton.Clicked += OnMinimizeClicked;

		var restoreButton = new Button
		{
			AutomationId = "RestoreButton",
			Text = "Restore Window"
		};
		restoreButton.Clicked += OnRestoreClicked;

		var runTestButton = new Button
		{
			AutomationId = "RunTestButton",
			Text = "Run Minimize/Restore Test",
			BackgroundColor = Colors.Blue,
			TextColor = Colors.White
		};
		runTestButton.Clicked += OnRunTestClicked;

		var verifyButton = new Button
		{
			AutomationId = "VerifyButton",
			Text = "Verify Results"
		};
		verifyButton.Clicked += OnVerifyClicked;

		var instructions = new Label
		{
			Text = "This test verifies Window.Activated and Window.Deactivated events fire correctly during minimize/restore cycles.\n\n" +
				   "Click 'Run Minimize/Restore Test' to run 2 minimize/restore cycles automatically.\n" +
				   "Then click 'Verify Results' to check the event counts.",
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center
		};

		Content = new VerticalStackLayout
		{
			Spacing = 15,
			Padding = 20,
			Children =
			{
				instructions,
				_statusLabel,
				_resultLabel,
				minimizeButton,
				restoreButton,
				runTestButton,
				verifyButton
			}
		};
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		
		_window = this.Window;
		if (_window != null)
		{
			_window.Activated += OnWindowActivated;
			_window.Deactivated += OnWindowDeactivated;
			_window.Resumed += OnWindowResumed;
		}
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);
		
		if (_window != null)
		{
			_window.Activated -= OnWindowActivated;
			_window.Deactivated -= OnWindowDeactivated;
			_window.Resumed -= OnWindowResumed;
		}
	}

	private void OnWindowActivated(object sender, EventArgs e)
	{
		_activated++;
		UpdateStatus();
	}

	private void OnWindowDeactivated(object sender, EventArgs e)
	{
		_deactivated++;
		UpdateStatus();
	}

	private void OnWindowResumed(object sender, EventArgs e)
	{
		_resumed++;
		UpdateStatus();
	}

	private void UpdateStatus()
	{
		_statusLabel.Text = $"Activated: {_activated}, Deactivated: {_deactivated}, Resumed: {_resumed}";
	}

#if WINDOWS
	private IntPtr GetWindowHandle()
	{
		if (_window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window win)
		{
			return WinRT.Interop.WindowNative.GetWindowHandle(win);
		}
		return IntPtr.Zero;
	}
#endif

	private void OnMinimizeClicked(object sender, EventArgs e)
	{
#if WINDOWS
		var hwnd = GetWindowHandle();
		if (hwnd != IntPtr.Zero)
		{
			ShowWindow(hwnd, SW_MINIMIZE);
		}
#endif
	}

	private void OnRestoreClicked(object sender, EventArgs e)
	{
#if WINDOWS
		var hwnd = GetWindowHandle();
		if (hwnd != IntPtr.Zero)
		{
			ShowWindow(hwnd, SW_RESTORE);
		}
#endif
	}

	private async void OnRunTestClicked(object sender, EventArgs e)
	{
#if WINDOWS
		var hwnd = GetWindowHandle();
		if (hwnd == IntPtr.Zero)
		{
			_resultLabel.Text = "ERROR: No window handle";
			return;
		}

		// Reset counters
		_activated = 0;
		_deactivated = 0;
		_resumed = 0;
		UpdateStatus();

		// Wait for initial activation to settle
		await Task.Delay(500);
		_resultLabel.Text = "Running test...";

		// Run 2 minimize/restore cycles
		for (int i = 0; i < 2; i++)
		{
			ShowWindow(hwnd, SW_RESTORE);
			await Task.Delay(300);

			ShowWindow(hwnd, SW_MINIMIZE);
			await Task.Delay(300);
		}

		// Final restore
		ShowWindow(hwnd, SW_RESTORE);
		await Task.Delay(300);

		_resultLabel.Text = "Test complete - click Verify";
#else
		_resultLabel.Text = "Windows only";
		await Task.CompletedTask;
#endif
	}

	private void OnVerifyClicked(object sender, EventArgs e)
	{
		// Expected: activated=2, deactivated=2 after 2 minimize/restore cycles
		bool passed = _activated >= 2 && _deactivated >= 2;
		
		if (passed)
		{
			_resultLabel.Text = "PASSED";
			_resultLabel.TextColor = Colors.Green;
		}
		else
		{
			_resultLabel.Text = $"FAILED: activated={_activated}, deactivated={_deactivated}";
			_resultLabel.TextColor = Colors.Red;
		}
	}
}
