namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea attached property for per-edge safe area control", PlatformAffected.All)]
public partial class Issue28986 : ContentPage
{
	public Issue28986()
	{
		InitializeComponent();
		// Initialize page-level controls (start with all ignore - True)
		PageLeftCheckBox.IsChecked = true;
		PageTopCheckBox.IsChecked = true;
		PageRightCheckBox.IsChecked = true;
		PageBottomCheckBox.IsChecked = true;
		
		// Initialize grid-level controls (start with all respect - False)
		GridLeftCheckBox.IsChecked = false;
		GridTopCheckBox.IsChecked = false;
		GridRightCheckBox.IsChecked = false;
		GridBottomCheckBox.IsChecked = false;
		
		UpdateSafeAreaSettings();
	}

	// Page-level event handlers
	private void OnPageEdgeCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		UpdateSafeAreaSettings();
	}

	private void OnPageResetAllClicked(object sender, EventArgs e)
	{
		PageLeftCheckBox.IsChecked = true;
		PageTopCheckBox.IsChecked = true;
		PageRightCheckBox.IsChecked = true;
		PageBottomCheckBox.IsChecked = true;
		UpdateSafeAreaSettings();
	}

	private void OnPageResetNoneClicked(object sender, EventArgs e)
	{
		PageLeftCheckBox.IsChecked = false;
		PageTopCheckBox.IsChecked = false;
		PageRightCheckBox.IsChecked = false;
		PageBottomCheckBox.IsChecked = false;
		UpdateSafeAreaSettings();
	}

	// Grid-level event handlers
	private void OnGridEdgeCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		UpdateSafeAreaSettings();
	}

	private void OnGridResetAllClicked(object sender, EventArgs e)
	{
		GridLeftCheckBox.IsChecked = true;
		GridTopCheckBox.IsChecked = true;
		GridRightCheckBox.IsChecked = true;
		GridBottomCheckBox.IsChecked = true;
		UpdateSafeAreaSettings();
	}

	private void OnGridResetNoneClicked(object sender, EventArgs e)
	{
		GridLeftCheckBox.IsChecked = false;
		GridTopCheckBox.IsChecked = false;
		GridRightCheckBox.IsChecked = false;
		GridBottomCheckBox.IsChecked = false;
		UpdateSafeAreaSettings();
	}

	private void UpdateSafeAreaSettings()
	{
		// Create SafeAreaEdges for page-level settings
		var pageSafeAreaSettings = new SafeAreaEdges(
			PageLeftCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,   // Left
			PageTopCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,    // Top  
			PageRightCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,  // Right
			PageBottomCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None  // Bottom
		);

		// Create SafeAreaEdges for grid-level settings
		var gridSafeAreaSettings = new SafeAreaEdges(
			GridLeftCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,   // Left
			GridTopCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,    // Top  
			GridRightCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,  // Right
			GridBottomCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None  // Bottom
		);

		// Apply SafeArea attached property to the ContentPage
		SafeArea.SetIgnoreSafeArea(this, pageSafeAreaSettings);

		// Apply SafeArea attached property to the main content grid
		SafeArea.SetIgnoreSafeArea(ContentGrid, gridSafeAreaSettings);

		// Update the display label
		var pageSettingsText = $"Page - Left: {(PageLeftCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                      $"Top: {(PageTopCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                      $"Right: {(PageRightCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                      $"Bottom: {(PageBottomCheckBox.IsChecked ? "Ignore" : "Respect")}";

		var gridSettingsText = $"Grid - Left: {(GridLeftCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                      $"Top: {(GridTopCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                      $"Right: {(GridRightCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                      $"Bottom: {(GridBottomCheckBox.IsChecked ? "Ignore" : "Respect")}";
		
		CurrentSettingsLabel.Text = $"Current:\n{pageSettingsText}\n{gridSettingsText}";

		// Test different syntax formats based on the current selections
		ApplyOptimizedSyntax(pageSafeAreaSettings, gridSafeAreaSettings);
	}

	private void ApplyOptimizedSyntax(SafeAreaEdges pageSettings, SafeAreaEdges gridSettings)
	{
		// Optimize page-level syntax when possible to test different input formats
		var pageLeft = pageSettings.Left;
		var pageTop = pageSettings.Top; 
		var pageRight = pageSettings.Right;
		var pageBottom = pageSettings.Bottom;

		SafeAreaEdges optimizedPageSettings;

		// Test 1-value syntax (all edges same)
		if (pageLeft == pageTop && pageTop == pageRight && pageRight == pageBottom)
		{
			optimizedPageSettings = new SafeAreaEdges(pageLeft);
		}
		// Test 2-value syntax (left/right same, top/bottom same)  
		else if (pageLeft == pageRight && pageTop == pageBottom)
		{
			optimizedPageSettings = new SafeAreaEdges(pageLeft, pageTop);
		}
		// Use 4-value syntax
		else
		{
			optimizedPageSettings = pageSettings;
		}

		// Optimize grid-level syntax when possible
		var gridLeft = gridSettings.Left;
		var gridTop = gridSettings.Top; 
		var gridRight = gridSettings.Right;
		var gridBottom = gridSettings.Bottom;

		SafeAreaEdges optimizedGridSettings;

		// Test 1-value syntax (all edges same)
		if (gridLeft == gridTop && gridTop == gridRight && gridRight == gridBottom)
		{
			optimizedGridSettings = new SafeAreaEdges(gridLeft);
		}
		// Test 2-value syntax (left/right same, top/bottom same)  
		else if (gridLeft == gridRight && gridTop == gridBottom)
		{
			optimizedGridSettings = new SafeAreaEdges(gridLeft, gridTop);
		}
		// Use 4-value syntax
		else
		{
			optimizedGridSettings = gridSettings;
		}

		// Apply the optimized settings to test different syntax formats
		SafeArea.SetIgnoreSafeArea(this, optimizedPageSettings);
		SafeArea.SetIgnoreSafeArea(ContentGrid, optimizedGridSettings);

		// Update the label to show which syntax is being used
		var pageSyntaxInfo = (pageLeft == pageTop && pageTop == pageRight && pageRight == pageBottom) ? " (1-value)" :
		                    (pageLeft == pageRight && pageTop == pageBottom) ? " (2-value)" : 
		                    " (4-value)";

		var gridSyntaxInfo = (gridLeft == gridTop && gridTop == gridRight && gridRight == gridBottom) ? " (1-value)" :
		                    (gridLeft == gridRight && gridTop == gridBottom) ? " (2-value)" : 
		                    " (4-value)";

		CurrentSettingsLabel.Text += $"\nPage{pageSyntaxInfo}, Grid{gridSyntaxInfo}";
	}
}