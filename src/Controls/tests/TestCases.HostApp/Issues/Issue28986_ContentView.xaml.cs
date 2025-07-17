namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea ContentView for per-edge safe area control", PlatformAffected.All)]
public partial class Issue28986_ContentView : ContentPage
{
	public Issue28986_ContentView()
	{
		InitializeComponent();
		// Initialize controls (start with all All)
		LeftPicker.SelectedIndex = 3;   // All
		TopPicker.SelectedIndex = 3;    // All
		RightPicker.SelectedIndex = 3;  // All
		BottomPicker.SelectedIndex = 3; // All
		
		UpdateSafeAreaSettings();
	}

	private void OnEdgePickerChanged(object sender, EventArgs e)
	{
		UpdateSafeAreaSettings();
	}

	private void OnResetDefaultClicked(object sender, EventArgs e)
	{
		LeftPicker.SelectedIndex = 0;   // Default
		TopPicker.SelectedIndex = 0;    // Default
		RightPicker.SelectedIndex = 0;  // Default
		BottomPicker.SelectedIndex = 0; // Default
		UpdateSafeAreaSettings();
	}

	private void OnResetNoneClicked(object sender, EventArgs e)
	{
		LeftPicker.SelectedIndex = 2;   // None
		TopPicker.SelectedIndex = 2;    // None
		RightPicker.SelectedIndex = 2;  // None
		BottomPicker.SelectedIndex = 2; // None
		UpdateSafeAreaSettings();
	}

	private void OnResetAllClicked(object sender, EventArgs e)
	{
		LeftPicker.SelectedIndex = 3;   // All
		TopPicker.SelectedIndex = 3;    // All
		RightPicker.SelectedIndex = 3;  // All
		BottomPicker.SelectedIndex = 3; // All
		UpdateSafeAreaSettings();
	}

	private SafeAreaRegions GetSafeAreaRegionsFromPickerIndex(int index)
	{
		return index switch
		{
			0 => SafeAreaRegions.Default,
			1 => SafeAreaRegions.SoftInput,
			2 => SafeAreaRegions.None,
			3 => SafeAreaRegions.All,
			_ => SafeAreaRegions.Default
		};
	}

	private void UpdateSafeAreaSettings()
	{
		// Create SafeAreaEdges for ContentView settings
		var safeAreaSettings = new SafeAreaEdges(
			GetSafeAreaRegionsFromPickerIndex(LeftPicker.SelectedIndex),   // Left
			GetSafeAreaRegionsFromPickerIndex(TopPicker.SelectedIndex),    // Top  
			GetSafeAreaRegionsFromPickerIndex(RightPicker.SelectedIndex),  // Right
			GetSafeAreaRegionsFromPickerIndex(BottomPicker.SelectedIndex)  // Bottom
		);

		// Apply SafeAreaIgnore to the ContentView
		TestContentView.SafeAreaIgnore = safeAreaSettings;

		// Update the display label
		var settingsText = $"Left: {GetSafeAreaRegionsFromPickerIndex(LeftPicker.SelectedIndex)}, " +
		                  $"Top: {GetSafeAreaRegionsFromPickerIndex(TopPicker.SelectedIndex)}, " +
		                  $"Right: {GetSafeAreaRegionsFromPickerIndex(RightPicker.SelectedIndex)}, " +
		                  $"Bottom: {GetSafeAreaRegionsFromPickerIndex(BottomPicker.SelectedIndex)}";
		
		CurrentSettingsLabel.Text = $"Current ContentView SafeAreaIgnore: {settingsText}";
	}
}