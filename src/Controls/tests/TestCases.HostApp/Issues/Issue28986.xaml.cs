namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeAreaGuides attached property for per-edge safe area control", PlatformAffected.All)]
public partial class Issue28986 : ContentPage
{
	public Issue28986()
	{
		InitializeComponent();
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
		// Create the SafeAreaGroup array based on checkbox states
		// Order: Left, Top, Right, Bottom
		var safeAreaSettings = new SafeAreaGroup[]
		{
			LeftCheckBox.IsChecked ? SafeAreaGroup.All : SafeAreaGroup.None,   // Left
			TopCheckBox.IsChecked ? SafeAreaGroup.All : SafeAreaGroup.None,    // Top  
			RightCheckBox.IsChecked ? SafeAreaGroup.All : SafeAreaGroup.None,  // Right
			BottomCheckBox.IsChecked ? SafeAreaGroup.All : SafeAreaGroup.None  // Bottom
		};

		// Apply the SafeAreaGuides attached property to the main content grid
		SafeAreaGuides.SetIgnoreSafeArea(ContentGrid, safeAreaSettings);

		// Update the display label
		var settingsText = $"Left: {(LeftCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                  $"Top: {(TopCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                  $"Right: {(RightCheckBox.IsChecked ? "Ignore" : "Respect")}, " +
		                  $"Bottom: {(BottomCheckBox.IsChecked ? "Ignore" : "Respect")}";
		
		CurrentSettingsLabel.Text = $"Current: {settingsText}";

		// Test different array syntaxes based on the current selection
		ApplyOptimizedSyntax(safeAreaSettings);
	}

	private void ApplyOptimizedSyntax(SafeAreaGroup[] settings)
	{
		// Optimize the array syntax when possible to test different input formats
		var left = settings[0];
		var top = settings[1]; 
		var right = settings[2];
		var bottom = settings[3];

		SafeAreaGroup[] optimizedSettings;

		// Test 1-value syntax (all edges same)
		if (left == top && top == right && right == bottom)
		{
			optimizedSettings = new[] { left };
		}
		// Test 2-value syntax (left/right same, top/bottom same)  
		else if (left == right && top == bottom)
		{
			optimizedSettings = new[] { left, top };
		}
		// Use 4-value syntax
		else
		{
			optimizedSettings = settings;
		}

		// Apply the optimized settings to test array interpretation
		SafeAreaGuides.SetIgnoreSafeArea(ContentGrid, optimizedSettings);

		// Update the label to show which syntax is being used
		var syntaxInfo = optimizedSettings.Length switch
		{
			1 => " (1-value syntax)",
			2 => " (2-value syntax)", 
			4 => " (4-value syntax)",
			_ => " (custom syntax)"
		};

		CurrentSettingsLabel.Text += syntaxInfo;
	}
}