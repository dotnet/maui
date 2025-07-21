namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea attached property for per-edge safe area control", PlatformAffected.All)]
public partial class Issue28986 : ContentPage
{
	public Issue28986()
	{
		InitializeComponent();
		// Initialize page-level controls (start with all None)
		PageLeftPicker.SelectedIndex = 0;   // None
		PageTopPicker.SelectedIndex = 0;    // None
		PageRightPicker.SelectedIndex = 0;  // None
		PageBottomPicker.SelectedIndex = 0; // None
		
		// Initialize grid-level controls (start with all None)
		GridLeftPicker.SelectedIndex = 0;   // None
		GridTopPicker.SelectedIndex = 0;    // None
		GridRightPicker.SelectedIndex = 0;  // None
		GridBottomPicker.SelectedIndex = 0; // None
		
		UpdateSafeAreaSettings();
	}

	// Page-level event handlers
	private void OnPageEdgePickerChanged(object sender, EventArgs e)
	{
		UpdateSafeAreaSettings();
	}

	private void OnPageResetDefaultClicked(object sender, EventArgs e)
	{
		PageLeftPicker.SelectedIndex = 0;   // None
		PageTopPicker.SelectedIndex = 0;    // None
		PageRightPicker.SelectedIndex = 0;  // None
		PageBottomPicker.SelectedIndex = 0; // None
		UpdateSafeAreaSettings();
	}

	private void OnPageResetNoneClicked(object sender, EventArgs e)
	{
		PageLeftPicker.SelectedIndex = 0;   // None
		PageTopPicker.SelectedIndex = 0;    // None
		PageRightPicker.SelectedIndex = 0;  // None
		PageBottomPicker.SelectedIndex = 0; // None
		UpdateSafeAreaSettings();
	}

	private void OnPageResetAllClicked(object sender, EventArgs e)
	{
		PageLeftPicker.SelectedIndex = 3;   // All
		PageTopPicker.SelectedIndex = 3;    // All
		PageRightPicker.SelectedIndex = 3;  // All
		PageBottomPicker.SelectedIndex = 3; // All
		UpdateSafeAreaSettings();
	}

	// Grid-level event handlers
	private void OnGridEdgePickerChanged(object sender, EventArgs e)
	{
		UpdateSafeAreaSettings();
	}

	private void OnGridResetDefaultClicked(object sender, EventArgs e)
	{
		GridLeftPicker.SelectedIndex = 0;   // None
		GridTopPicker.SelectedIndex = 0;    // None
		GridRightPicker.SelectedIndex = 0;  // None
		GridBottomPicker.SelectedIndex = 0; // None
		UpdateSafeAreaSettings();
	}

	private void OnGridResetNoneClicked(object sender, EventArgs e)
	{
		GridLeftPicker.SelectedIndex = 0;   // None
		GridTopPicker.SelectedIndex = 0;    // None
		GridRightPicker.SelectedIndex = 0;  // None
		GridBottomPicker.SelectedIndex = 0; // None
		UpdateSafeAreaSettings();
	}

	private void OnGridResetAllClicked(object sender, EventArgs e)
	{
		GridLeftPicker.SelectedIndex = 3;   // All
		GridTopPicker.SelectedIndex = 3;    // All
		GridRightPicker.SelectedIndex = 3;  // All
		GridBottomPicker.SelectedIndex = 3; // All
		UpdateSafeAreaSettings();
	}

	private SafeAreaRegions GetSafeAreaRegionsFromPickerIndex(int index)
	{
		return index switch
		{
			0 => SafeAreaRegions.None,
			1 => SafeAreaRegions.Keyboard,
			2 => SafeAreaRegions.Container,
			3 => SafeAreaRegions.All,
			_ => SafeAreaRegions.None
		};
	}

	private void UpdateSafeAreaSettings()
	{
		// Create SafeAreaEdges for page-level settings
		var pageSafeAreaSettings = new SafeAreaEdges(
			GetSafeAreaRegionsFromPickerIndex(PageLeftPicker.SelectedIndex),   // Left
			GetSafeAreaRegionsFromPickerIndex(PageTopPicker.SelectedIndex),    // Top  
			GetSafeAreaRegionsFromPickerIndex(PageRightPicker.SelectedIndex),  // Right
			GetSafeAreaRegionsFromPickerIndex(PageBottomPicker.SelectedIndex)  // Bottom
		);

		// Create SafeAreaEdges for grid-level settings
		var gridSafeAreaSettings = new SafeAreaEdges(
			GetSafeAreaRegionsFromPickerIndex(GridLeftPicker.SelectedIndex),   // Left
			GetSafeAreaRegionsFromPickerIndex(GridTopPicker.SelectedIndex),    // Top  
			GetSafeAreaRegionsFromPickerIndex(GridRightPicker.SelectedIndex),  // Right
			GetSafeAreaRegionsFromPickerIndex(GridBottomPicker.SelectedIndex)  // Bottom
		);

		// Apply SafeAreaEdges property to the ContentPage
		this.SafeAreaEdges = pageSafeAreaSettings;

		// Apply SafeAreaEdges property to the main content grid
		ContentGrid.SafeAreaEdges = gridSafeAreaSettings;

		// Update the display label
		var pageSettingsText = $"Page - Left: {GetSafeAreaRegionsFromPickerIndex(PageLeftPicker.SelectedIndex)}, " +
		                      $"Top: {GetSafeAreaRegionsFromPickerIndex(PageTopPicker.SelectedIndex)}, " +
		                      $"Right: {GetSafeAreaRegionsFromPickerIndex(PageRightPicker.SelectedIndex)}, " +
		                      $"Bottom: {GetSafeAreaRegionsFromPickerIndex(PageBottomPicker.SelectedIndex)}";

		var gridSettingsText = $"Grid - Left: {GetSafeAreaRegionsFromPickerIndex(GridLeftPicker.SelectedIndex)}, " +
		                      $"Top: {GetSafeAreaRegionsFromPickerIndex(GridTopPicker.SelectedIndex)}, " +
		                      $"Right: {GetSafeAreaRegionsFromPickerIndex(GridRightPicker.SelectedIndex)}, " +
		                      $"Bottom: {GetSafeAreaRegionsFromPickerIndex(GridBottomPicker.SelectedIndex)}";
		
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
		this.SafeAreaEdges = optimizedPageSettings;
		ContentGrid.SafeAreaEdges = optimizedGridSettings;

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