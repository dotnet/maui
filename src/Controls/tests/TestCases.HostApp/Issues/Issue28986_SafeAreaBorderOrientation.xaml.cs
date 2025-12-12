namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 5)]
public partial class Issue28986_SafeAreaBorderOrientation : ContentPage
{
	private bool _borderUsesAll = true;

	public Issue28986_SafeAreaBorderOrientation()
	{
		InitializeComponent();

		// Monitor entry focus for keyboard detection
		TestEntry.Focused += OnEntryFocused;
		TestEntry.Unfocused += OnEntryUnfocused;

		// Monitor size changes for orientation detection
		this.SizeChanged += OnSizeChanged;

		// Update dimensions when the page appears
		this.Appearing += OnPageAppearing;
	}

	private void OnPageAppearing(object sender, EventArgs e)
	{
		// Delay to allow layout to complete
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
		{
			UpdateOrientationInfo();
			UpdateBorderDimensions();
		});
	}

	private void OnSizeChanged(object sender, EventArgs e)
	{
		// Update orientation info when size changes (indicating orientation change)
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
		{
			UpdateOrientationInfo();
			UpdateBorderDimensions();
		});
	}

	private void OnEntryFocused(object sender, FocusEventArgs e)
	{
		KeyboardStatusLabel.Text = "Keyboard Status: Visible";
		TestStatusLabel.Text = "Test Status: Keyboard shown, checking safe area adjustment";

		// Update dimensions after keyboard appears - multiple delays to catch all changes
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), UpdateBorderDimensions);
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), UpdateBorderDimensions);
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), UpdateBorderDimensions);
	}

	private void OnEntryUnfocused(object sender, FocusEventArgs e)
	{
		KeyboardStatusLabel.Text = "Keyboard Status: Hidden";
		TestStatusLabel.Text = "Test Status: Keyboard hidden, checking safe area restoration";

		// Update dimensions after keyboard hides - multiple delays to catch all changes
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), UpdateBorderDimensions);
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), UpdateBorderDimensions);
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), UpdateBorderDimensions);
	}

	private void OnSimulateOrientationClicked(object sender, EventArgs e)
	{
		// This button is now deprecated in favor of real orientation changes
		TestStatusLabel.Text = "Test Status: Use real orientation changes via SetOrientationLandscape/SetOrientationPortrait in tests";

		// Update display to show current actual orientation
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
		{
			UpdateOrientationInfo();
			UpdateBorderDimensions();
		});
	}

	private void OnResetToPortraitClicked(object sender, EventArgs e)
	{
		// This button is now informational only
		TestStatusLabel.Text = "Test Status: Use SetOrientationPortrait() in test code for actual orientation reset";

		// Update display to show current actual orientation
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
		{
			UpdateOrientationInfo();
			UpdateBorderDimensions();
		});
	}

	private void OnToggleSafeAreaEdgesClicked(object sender, EventArgs e)
	{
		_borderUsesAll = !_borderUsesAll;
		TestBorder.SafeAreaEdges = _borderUsesAll ? SafeAreaEdges.All : SafeAreaEdges.None;
		TestStatusLabel.Text = $"Test Status: SafeAreaEdges changed to {(_borderUsesAll ? "All" : "None")}";

		// Update dimensions after safe area change
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), UpdateBorderDimensions);
	}

	private void OnSetBottomSoftInputClicked(object sender, EventArgs e)
	{
		var currentEdges = TestBorder.SafeAreaEdges;
		TestBorder.SafeAreaEdges = new SafeAreaEdges(
			currentEdges.Left,
			currentEdges.Top,
			currentEdges.Right,
			SafeAreaRegions.SoftInput
		);
		TestStatusLabel.Text = "Test Status: Bottom edge set to SoftInput";

		// Update dimensions after safe area change
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), UpdateBorderDimensions);
	}

	private void OnSetBottomAllClicked(object sender, EventArgs e)
	{
		var currentEdges = TestBorder.SafeAreaEdges;
		TestBorder.SafeAreaEdges = new SafeAreaEdges(
			currentEdges.Left,
			currentEdges.Top,
			currentEdges.Right,
			SafeAreaRegions.All
		);
		TestStatusLabel.Text = "Test Status: Bottom edge set to All";

		// Update dimensions after safe area change
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), UpdateBorderDimensions);
	}

	private void UpdateOrientationInfo()
	{
		try
		{
			// Determine orientation based on actual page dimensions
			var isLandscape = this.Width > this.Height;
			var orientationText = isLandscape ? "Landscape" : "Portrait";

			CurrentOrientationLabel.Text = $"Current Orientation: {orientationText}";

			// Also update test status with actual orientation
			if (TestStatusLabel.Text.Contains("orientation", StringComparison.OrdinalIgnoreCase))
			{
				TestStatusLabel.Text = $"Test Status: Current orientation is {orientationText}";
			}
		}
		catch (Exception ex)
		{
			CurrentOrientationLabel.Text = $"Current Orientation: Error - {ex.Message}";
		}
	}

	private void UpdateBorderDimensions()
	{
		try
		{
			// Get border content bounds
			var borderBounds = BorderContent.Bounds;
			BorderDimensionsLabel.Text = $"Border Dimensions: {borderBounds.Width:F1} x {borderBounds.Height:F1} at ({borderBounds.X:F1}, {borderBounds.Y:F1})";

			// Get safe area information (platform-specific)
			var safeAreaInfo = GetSafeAreaInfo();
			SafeAreaInsetsLabel.Text = $"Safe Area Insets: {safeAreaInfo}";
		}
		catch (Exception ex)
		{
			BorderDimensionsLabel.Text = $"Border Dimensions: Error - {ex.Message}";
			SafeAreaInsetsLabel.Text = "Safe Area Insets: Error getting insets";
		}
	}

	private string GetSafeAreaInfo()
	{
#if ANDROID
        try
        {
            if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView is Android.Views.View decorView)
            {
                var insets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(decorView);
                if (insets != null)
                {
                    var systemBars = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars());
                    var ime = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.Ime());
                    var displayCutout = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout());
                    
                    return $"SystemBars: L{systemBars.Left} T{systemBars.Top} R{systemBars.Right} B{systemBars.Bottom}, " +
                           $"IME: B{ime.Bottom}, Cutout: L{displayCutout.Left} T{displayCutout.Top} R{displayCutout.Right} B{displayCutout.Bottom}";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Android Error: {ex.Message}";
        }
#elif IOS || MACCATALYST
		try
		{
			// Use the Handler to get platform-specific safe area information
			if (this.Handler?.PlatformView is UIKit.UIView platformView &&
				platformView.Window != null)
			{
				var safeAreaInsets = platformView.Window.SafeAreaInsets;

				// On iOS, also check the view's own frame and bounds to detect keyboard impact
				var keyboardHeight = 0.0;
				var frameInfo = "";
				try
				{
					// Check if the view's frame suggests keyboard is present
					var window = platformView.Window;
					var windowBounds = window.Bounds;
					var viewFrame = platformView.Frame;

					frameInfo = $" Window:{windowBounds.Height:F0} View:{viewFrame.Height:F0}";

					// Calculate expected vs actual available height
					var expectedHeight = windowBounds.Height - safeAreaInsets.Top - safeAreaInsets.Bottom;
					var actualAvailableHeight = viewFrame.Height;

					// If there's a significant difference, likely keyboard is showing
					if (expectedHeight - actualAvailableHeight > 50) // 50 pt threshold
					{
						keyboardHeight = expectedHeight - actualAvailableHeight;
					}
				}
				catch (Exception ex)
				{
					frameInfo = $" FrameError:{ex.Message}";
				}

				// Include keyboard effect in bottom inset
				var effectiveBottomInset = safeAreaInsets.Bottom + keyboardHeight;
				return $"iOS SafeArea: L{safeAreaInsets.Left:F1} T{safeAreaInsets.Top:F1} R{safeAreaInsets.Right:F1} B{effectiveBottomInset:F1}" +
					   (keyboardHeight > 0 ? $" (KB:{keyboardHeight:F1})" : "") + frameInfo;
			}
		}
		catch (Exception ex)
		{
			return $"iOS Error: {ex.Message}";
		}
#endif
		// Try to get basic safe area information from the current page and border
		try
		{
			var pagePadding = this.Padding;
			var borderBounds = TestBorder?.Bounds ?? new Rect();
			var borderSafeArea = TestBorder?.SafeAreaEdges ?? SafeAreaEdges.None;

			return $"Page Padding: L{pagePadding.Left:F1} T{pagePadding.Top:F1} R{pagePadding.Right:F1} B{pagePadding.Bottom:F1}, " +
				   $"Border: {borderBounds.Width:F0}x{borderBounds.Height:F0}, SafeEdges: {borderSafeArea}";
		}
		catch
		{
			return "Safe area information unavailable";
		}
	}
}
