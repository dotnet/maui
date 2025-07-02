namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeAreaGuides attached property for per-edge safe area control", PlatformAffected.All)]
public partial class Issue28986 : ContentPage
{
	public Issue28986()
	{
		InitializeComponent();
		// Since SafeAreaGuides default is now None, start with all unchecked
		// This demonstrates the new default behavior
		LeftCheckBox.IsChecked = false;
		TopCheckBox.IsChecked = false;
		RightCheckBox.IsChecked = false;
		BottomCheckBox.IsChecked = false;
		UpdateSafeAreaSettings();
	}

	private void OnEdgeCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		UpdateSafeAreaSettings();
	}

	private void OnResetAllClicked(object sender, EventArgs e)
	{
		LeftCheckBox.IsChecked = true;
		TopCheckBox.IsChecked = true;
		RightCheckBox.IsChecked = true;
		BottomCheckBox.IsChecked = true;
		UpdateSafeAreaSettings();
	}

	private void OnResetNoneClicked(object sender, EventArgs e)
	{
		LeftCheckBox.IsChecked = false;
		TopCheckBox.IsChecked = false;
		RightCheckBox.IsChecked = false;
		BottomCheckBox.IsChecked = false;
		UpdateSafeAreaSettings();
	}

	private void UpdateSafeAreaSettings()
	{
		// Create the SafeAreaEdges based on checkbox states
		var safeAreaSettings = new SafeAreaEdges(
			LeftCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,   // Left
			TopCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,    // Top  
			RightCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None,  // Right
			BottomCheckBox.IsChecked ? SafeAreaRegions.All : SafeAreaRegions.None  // Bottom
		);

		// Apply the SafeAreaGuides attached property to the main content grid
		SafeAreaGuides.SetIgnoreSafeArea(ContentGrid, safeAreaSettings);

		// Update the display label
		var settingsText = $"Left: {(LeftCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                  $"Top: {(TopCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                  $"Right: {(RightCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                  $"Bottom: {(BottomCheckBox.IsChecked ? "Ignore" : "Respect")}";
		
		CurrentSettingsLabel.Text = $"Current: {settingsText}";

		// Test different syntax formats based on the current selection
		ApplyOptimizedSyntax(safeAreaSettings);
	}

	private void ApplyOptimizedSyntax(SafeAreaEdges settings)
	{
		// Optimize the syntax when possible to test different input formats
		var left = settings.Left;
		var top = settings.Top; 
		var right = settings.Right;
		var bottom = settings.Bottom;

		SafeAreaEdges optimizedSettings;

		// Test 1-value syntax (all edges same)
		if (left == top && top == right && right == bottom)
		{
			optimizedSettings = new SafeAreaEdges(left);
		}
		// Test 2-value syntax (left/right same, top/bottom same)  
		else if (left == right && top == bottom)
		{
			optimizedSettings = new SafeAreaEdges(left, top);
		}
		// Use 4-value syntax
		else
		{
			optimizedSettings = settings;
		}

		// Apply the optimized settings to test different syntax formats
		SafeAreaGuides.SetIgnoreSafeArea(ContentGrid, optimizedSettings);

		// Update the label to show which syntax is being used
		var syntaxInfo = (left == top && top == right && right == bottom) ? " (1-value syntax)" :
		                (left == right && top == bottom) ? " (2-value syntax)" : 
		                " (4-value syntax)";

		CurrentSettingsLabel.Text += syntaxInfo;
	}
}