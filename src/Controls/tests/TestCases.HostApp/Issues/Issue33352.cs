using System;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.ApplicationModel;
#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using UIKit;
using System.Linq;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33352, "Intermittent crash on exit on MacCatalyst - ObjectDisposedException in ShellSectionRootRenderer", PlatformAffected.macOS)]
public class Issue33352 : ContentPage
{
	Label _statusLabel;
	Label _resultLabel;
	int _themeChangeCount = 0;
	static int _windowCloseCount = 0;
	static bool _exceptionOccurred = false;
	static string _exceptionMessage = "";
	
	public Issue33352()
	{
		
		_statusLabel = new Label
		{
			Text = "Ready - Tap buttons to test theme changes during window close",
			AutomationId = "StatusLabel",
			HorizontalOptions = LayoutOptions.Center
		};
		
		var changeThemeButton = new Button
		{
			Text = "Change Theme",
			AutomationId = "ChangeThemeButton"
		};
		changeThemeButton.Clicked += OnChangeThemeClicked;
		
		var triggerTraitChangeButton = new Button
		{
			Text = "Trigger Rapid Theme Changes",
			AutomationId = "TriggerTraitChangeButton"
		};
		triggerTraitChangeButton.Clicked += OnTriggerTraitChangeClicked;
		
		// This is the key button - opens a new window with Shell, then closes it while changing themes
		var openAndCloseWindowButton = new Button
		{
			Text = "Open Shell Window, Change Theme, Close",
			AutomationId = "OpenCloseWindowButton",
			BackgroundColor = Colors.Orange,
			TextColor = Colors.White
		};
		openAndCloseWindowButton.Clicked += OnOpenAndCloseWindowClicked;
		
		var themeChangeCountLabel = new Label
		{
			Text = "Theme changes: 0",
			AutomationId = "ThemeChangeCountLabel"
		};
		
		var windowCloseCountLabel = new Label
		{
			Text = "Window closes: 0",
			AutomationId = "WindowCloseCountLabel"
		};
		
		// Observe application theme changes
		if (Application.Current != null)
		{
			Application.Current.RequestedThemeChanged += (s, e) =>
			{
				_themeChangeCount++;
				themeChangeCountLabel.Text = $"Theme changes: {_themeChangeCount}";
			};
		}
		
		var instructionsLabel = new Label
		{
			Text = "This test opens a new window with Shell, changes the theme, then closes the window. " +
				   "The bug causes ObjectDisposedException when TraitCollectionDidChange is called during window disposal.",
			AutomationId = "InstructionsLabel",
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center,
			Margin = new Thickness(20)
		};
		
		var successLabel = new Label
		{
			Text = "Test Ready",
			AutomationId = "SuccessLabel",
			TextColor = Colors.Green,
			FontSize = 24,
			HorizontalOptions = LayoutOptions.Center
		};
		
		_resultLabel = new Label
		{
			Text = "",
			AutomationId = "ResultLabel",
			HorizontalOptions = LayoutOptions.Center
		};
		
		// This button directly tests the race condition by calling TraitCollectionDidChange on a disposed renderer
		var testDisposedRendererButton = new Button
		{
			Text = "Test TraitCollectionDidChange After Dispose",
			AutomationId = "TestDisposedRendererButton",
			BackgroundColor = Colors.Red,
			TextColor = Colors.White
		};
		testDisposedRendererButton.Clicked += OnTestDisposedRendererClicked;
		
		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = 20,
				Children =
				{
					instructionsLabel,
					_statusLabel,
					_resultLabel,
					testDisposedRendererButton,
					changeThemeButton,
					triggerTraitChangeButton,
					openAndCloseWindowButton,
					themeChangeCountLabel,
					windowCloseCountLabel,
					successLabel
				}
			}
		};
		
		// Update status periodically to show window close count
		var timer = Dispatcher.CreateTimer();
		timer.Interval = TimeSpan.FromMilliseconds(500);
		timer.Tick += (s, e) =>
		{
			windowCloseCountLabel.Text = $"Window closes: {_windowCloseCount}";
			if (_exceptionOccurred)
			{
				_statusLabel.Text = $"FAILED: {_exceptionMessage}";
				_statusLabel.TextColor = Colors.Red;
				successLabel.Text = "FAILED";
				successLabel.TextColor = Colors.Red;
			}
		};
		timer.Start();
		
		// Auto-run test after a short delay when page loads
		// This allows testing without Appium by just navigating to this page
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1000), async () =>
		{
			_statusLabel.Text = "Auto-running test...";
			OnTestDisposedRendererClicked(null, null);
		});
	}
	
	async void OnOpenAndCloseWindowClicked(object sender, EventArgs e)
	{
		_statusLabel.Text = "Opening new Shell window...";
		
		var app = Application.Current;
		if (app == null)
		{
			_statusLabel.Text = "FAILED: Application.Current is null";
			return;
		}
		
		// Create a Shell with multiple sections (this creates ShellSectionRootRenderer instances)
		var shellWindow = CreateShellForNewWindow();
		
		// Create a new window with the Shell
		var newWindow = new Window(shellWindow);
		
		app.OpenWindow(newWindow);
		_statusLabel.Text = "New window opened, waiting...";
		
		// Wait a moment for the window to fully initialize
		await Task.Delay(500);
		
		// Start changing themes rapidly in the background
		var themeChangeTask = Task.Run(async () =>
		{
			for (int i = 0; i < 30; i++)
			{
				await Task.Delay(30);
				MainThread.BeginInvokeOnMainThread(() =>
				{
					try
					{
						if (Application.Current != null)
						{
							Application.Current.UserAppTheme = Application.Current.UserAppTheme != AppTheme.Dark 
								? AppTheme.Dark 
								: AppTheme.Light;
						}
					}
					catch (ObjectDisposedException ex)
					{
						_exceptionOccurred = true;
						_exceptionMessage = $"ObjectDisposedException: {ex.Message}";
					}
					catch (Exception)
					{
					}
				});
			}
		});
		
		// Wait a bit, then close the window while theme changes are still happening
		await Task.Delay(300);
		_statusLabel.Text = "Closing window while changing themes...";
		
		try
		{
			app.CloseWindow(newWindow);
			_windowCloseCount++;
		}
		catch (ObjectDisposedException ex)
		{
			_exceptionOccurred = true;
			_exceptionMessage = $"ObjectDisposedException on close: {ex.Message}";
		}
		catch (Exception)
		{
		}
		
		// Wait for theme changes to complete
		await themeChangeTask;
		
		if (_exceptionOccurred)
		{
			_statusLabel.Text = $"FAILED: {_exceptionMessage}";
		}
		else
		{
			_statusLabel.Text = $"Window closed successfully. Close count: {_windowCloseCount}";
		}
		
	}
	
	Shell CreateShellForNewWindow()
	{
		var shell = new Shell();
		
		var mainPage = new ContentPage
		{
			Title = "Shell Window",
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label { Text = "This is a Shell in a new window", AutomationId = "NewWindowLabel" },
					new Label { Text = "This window will close while theme changes are happening", TextColor = Colors.Gray }
				}
			}
		};
		
		var shellContent = new ShellContent
		{
			Title = "Main",
			Content = mainPage,
			Route = "main"
		};
		
		var shellSection = new ShellSection
		{
			Title = "Tab1",
			Items = { shellContent }
		};
		
		// Add a second tab to ensure multiple ShellSectionRootRenderer instances
		var secondPage = new ContentPage
		{
			Title = "Second",
			Content = new Label { Text = "Second Tab", AutomationId = "SecondTabLabel" }
		};
		
		var secondContent = new ShellContent
		{
			Title = "Tab2",
			Content = secondPage,
			Route = "tab2"
		};
		
		var secondSection = new ShellSection
		{
			Title = "Tab2",
			Items = { secondContent }
		};
		
		var shellItem = new ShellItem
		{
			Items = { shellSection, secondSection }
		};
		
		shell.Items.Add(shellItem);
		
		return shell;
	}
	
	void OnChangeThemeClicked(object sender, EventArgs e)
	{
		try
		{
			if (Application.Current != null)
			{
				Application.Current.UserAppTheme = Application.Current.UserAppTheme != AppTheme.Dark 
					? AppTheme.Dark 
					: AppTheme.Light;
				
				_statusLabel.Text = $"Theme changed to {Application.Current.UserAppTheme}";
			}
		}
		catch (ObjectDisposedException ex)
		{
			_statusLabel.Text = $"FAILED: ObjectDisposedException - {ex.Message}";
			_exceptionOccurred = true;
			_exceptionMessage = ex.Message;
		}
		catch (Exception)
		{
			_statusLabel.Text = "FAILED: Exception";
		}
	}
	
	void OnTriggerTraitChangeClicked(object sender, EventArgs e)
	{
		try
		{
			for (int i = 0; i < 10; i++)
			{
				if (Application.Current != null)
				{
					Application.Current.UserAppTheme = Application.Current.UserAppTheme != AppTheme.Dark 
						? AppTheme.Dark 
						: AppTheme.Light;
				}
			}
			
			_statusLabel.Text = "Rapid theme changes completed successfully";
		}
		catch (ObjectDisposedException ex)
		{
			_statusLabel.Text = $"FAILED: ObjectDisposedException - {ex.Message}";
			_exceptionOccurred = true;
			_exceptionMessage = ex.Message;
		}
		catch (Exception)
		{
			_statusLabel.Text = "FAILED: Exception";
		}
	}
	
	/// <summary>
	/// This test directly reproduces the bug by:
	/// 1. Opening a Shell window
	/// 2. Capturing a reference to the ShellSectionRootRenderer
	/// 3. Closing the window (which disposes the renderer and services)
	/// 4. Calling TraitCollectionDidChange on the disposed renderer
	/// 
	/// Without the fix, this throws ObjectDisposedException.
	/// With the fix (removing TraitCollectionDidChange override), this should not crash.
	/// </summary>
	async void OnTestDisposedRendererClicked(object sender, EventArgs e)
	{
		_statusLabel.Text = "Testing TraitCollectionDidChange after dispose...";
		_resultLabel.Text = "";
		_resultLabel.TextColor = Colors.Black;
		
#if IOS || MACCATALYST
		await TestTraitCollectionDidChangeAfterDisposePlatform();
#else
		await Task.CompletedTask;
		_statusLabel.Text = "Test only runs on iOS/MacCatalyst";
		_resultLabel.Text = "SKIPPED";
		_resultLabel.TextColor = Colors.Gray;
#endif
	}
	
#if IOS || MACCATALYST
	async Task TestTraitCollectionDidChangeAfterDisposePlatform()
	{
		var app = Application.Current;
		if (app == null)
		{
			_statusLabel.Text = "FAILED: Application.Current is null";
			_resultLabel.Text = "FAILED";
			_resultLabel.TextColor = Colors.Red;
			return;
		}
		
		var mainWindow = app.Windows.FirstOrDefault();
		if (mainWindow == null)
		{
			_statusLabel.Text = "FAILED: No main window";
			_resultLabel.Text = "FAILED";
			_resultLabel.TextColor = Colors.Red;
			return;
		}
		
		// On MacCatalyst, we can open a new window with Shell, capture the renderer,
		// then close the window (which disposes the ServiceProvider), then call TraitCollectionDidChange
		
#if MACCATALYST
		// MacCatalyst supports multiple windows, so we can test the real scenario
		_statusLabel.Text = "Opening new Shell window...";
		
		var shell = CreateShellForNewWindow();
		var newWindow = new Window(shell);
		
		app.OpenWindow(newWindow);
		
		// Wait for the window to fully initialize
		await Task.Delay(1000);
		
		// Capture the ShellSectionRootRenderer AND PageViewController
		ShellSectionRootRenderer capturedRenderer = null;
		UIViewController capturedPageViewController = null;
		UITraitCollection previousTraitCollection = null;
		
		var handler = shell.Handler as ShellRenderer;
		if (handler != null)
		{
			try
			{
				IShellContext shellContext = handler;
				var shellItemRenderer = shellContext.CurrentShellItemRenderer as ShellItemRenderer;
				if (shellItemRenderer != null)
				{
					var sectionRenderer = shellItemRenderer.CurrentRenderer as ShellSectionRenderer;
					if (sectionRenderer?.ViewControllers != null)
					{
						capturedRenderer = sectionRenderer.ViewControllers
							.OfType<ShellSectionRootRenderer>()
							.FirstOrDefault();
						
						if (capturedRenderer != null)
						{
							previousTraitCollection = capturedRenderer.TraitCollection;
						}
					}
					
					// Also capture the PageViewController from the current page
					var currentPage = shell.CurrentPage;
					if (currentPage?.Handler is IPlatformViewHandler pageHandler)
					{
						capturedPageViewController = pageHandler.ViewController;
						if (capturedPageViewController != null)
						{
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}
		
		if (capturedRenderer == null && capturedPageViewController == null)
		{
			_statusLabel.Text = "Could not capture renderer/view controller";
			_resultLabel.Text = "SKIPPED - No renderer";
			_resultLabel.TextColor = Colors.Orange;
			try { app.CloseWindow(newWindow); } catch { }
			return;
		}
		
		// Now close the window - this will call DisposeWindowScope on the new window
		_statusLabel.Text = "Closing window (disposing ServiceProvider)...";
		
		app.CloseWindow(newWindow);
		
		// Wait for the window to be destroyed and scope disposed
		await Task.Delay(500);
		
		// Now try to call TraitCollectionDidChange on the captured (now disposed) renderers
		// This simulates what happens when iOS calls TraitCollectionDidChange after the scope is disposed
		_statusLabel.Text = "Calling TraitCollectionDidChange after disposal...";
		
		bool shellSectionRendererFailed = false;
		bool pageViewControllerFailed = false;
		string failureMessage = "";
		
		// Test 1: ShellSectionRootRenderer (if we captured it - this was removed in PR but we still want to verify it would crash without fix)
		if (capturedRenderer != null)
		{
			try
			{
#pragma warning disable CA1422 // Validate platform compatibility
				capturedRenderer.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
			}
			catch (ObjectDisposedException ex)
			{
				shellSectionRendererFailed = true;
				if (string.IsNullOrEmpty(failureMessage))
					failureMessage = $"ShellSectionRootRenderer: {ex.Message}";
			}
		}
		
		// Test 2: PageViewController (the actual fix location per PR #33353)
		if (capturedPageViewController != null)
		{
			try
			{
#pragma warning disable CA1422 // Validate platform compatibility
				capturedPageViewController.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
			}
			catch (ObjectDisposedException ex)
			{
				pageViewControllerFailed = true;
				if (string.IsNullOrEmpty(failureMessage))
					failureMessage = $"PageViewController: {ex.Message}";
				else
					failureMessage += $"; PageViewController: {ex.Message}";
			}
		}
		
		// Report results
		if (shellSectionRendererFailed || pageViewControllerFailed)
		{
			_statusLabel.Text = $"REPRODUCED: ObjectDisposedException";
			_resultLabel.Text = $"FAILED: {failureMessage}";
			_resultLabel.TextColor = Colors.Red;
			_exceptionOccurred = true;
			_exceptionMessage = failureMessage;
		}
		else
		{
			_statusLabel.Text = "Both TraitCollectionDidChange completed successfully";
			_resultLabel.Text = "SUCCESS";
			_resultLabel.TextColor = Colors.Green;
		}
#else
		// On iOS (iPhone), multi-window is not supported, so we use a page-swap approach
		// This doesn't perfectly reproduce the bug but tests that TraitCollectionDidChange
		// doesn't crash when called on a disconnected renderer
		
		var originalPage = mainWindow.Page;
		var shell = CreateShellForNewWindow();
		
		_statusLabel.Text = "Setting Shell as main page...";
		mainWindow.Page = shell;
		
		await Task.Delay(500);
		
		ShellSectionRootRenderer capturedRenderer = null;
		UITraitCollection previousTraitCollection = null;
		
		var handler = shell.Handler as ShellRenderer;
		if (handler != null)
		{
			try
			{
				IShellContext shellContext = handler;
				var shellItemRenderer = shellContext.CurrentShellItemRenderer as ShellItemRenderer;
				if (shellItemRenderer != null)
				{
					var sectionRenderer = shellItemRenderer.CurrentRenderer as ShellSectionRenderer;
					if (sectionRenderer?.ViewControllers != null)
					{
						capturedRenderer = sectionRenderer.ViewControllers
							.OfType<ShellSectionRootRenderer>()
							.FirstOrDefault();
						
						if (capturedRenderer != null)
						{
							previousTraitCollection = capturedRenderer.TraitCollection;
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}
		
		if (capturedRenderer == null)
		{
			_statusLabel.Text = "Could not capture renderer - restoring original page";
			mainWindow.Page = originalPage;
			_resultLabel.Text = "SKIPPED - No renderer";
			_resultLabel.TextColor = Colors.Orange;
			return;
		}
		
		// Restore original page - this disconnects the handler
		mainWindow.Page = originalPage;
		await Task.Delay(200);
		
		// Call TraitCollectionDidChange on the disconnected renderer
		
		try
		{
#pragma warning disable CA1422 // Validate platform compatibility
			capturedRenderer.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
			
			_statusLabel.Text = "TraitCollectionDidChange completed successfully";
			_resultLabel.Text = "SUCCESS";
			_resultLabel.TextColor = Colors.Green;
		}
		catch (ObjectDisposedException ex)
		{
			_statusLabel.Text = $"REPRODUCED: ObjectDisposedException";
			_resultLabel.Text = $"FAILED: {ex.Message}";
			_resultLabel.TextColor = Colors.Red;
			_exceptionOccurred = true;
			_exceptionMessage = ex.Message;
		}
		catch (Exception ex)
		{
			_statusLabel.Text = $"Exception: {ex.GetType().Name}";
			_resultLabel.Text = $"{ex.GetType().Name}: {ex.Message}";
			_resultLabel.TextColor = Colors.Orange;
		}
#endif
	}
#endif
}
