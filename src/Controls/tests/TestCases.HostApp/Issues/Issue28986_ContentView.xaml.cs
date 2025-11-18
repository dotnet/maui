#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea ContentView for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 3)]
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

#if ANDROID
		// Set SoftInput.AdjustNothing - we have full control over insets (iOS-like behavior)
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}

	private void OnEdgePickerChanged(object sender, EventArgs e)
	{
		UpdateSafeAreaSettings();
	}

	private void OnResetDefaultClicked(object sender, EventArgs e)
	{
		LeftPicker.SelectedIndex = 3;   // Default
		TopPicker.SelectedIndex = 3;    // Default
		RightPicker.SelectedIndex = 3;  // Default
		BottomPicker.SelectedIndex = 3; // Default
		UpdateSafeAreaSettings();
	}

	private void OnResetNoneClicked(object sender, EventArgs e)
	{
		LeftPicker.SelectedIndex = 0;   // None
		TopPicker.SelectedIndex = 0;    // None
		RightPicker.SelectedIndex = 0;  // None
		BottomPicker.SelectedIndex = 0; // None
		UpdateSafeAreaSettings();
	}

	private void OnResetAllClicked(object sender, EventArgs e)
	{
		LeftPicker.SelectedIndex = 4;   // All
		TopPicker.SelectedIndex = 4;    // All
		RightPicker.SelectedIndex = 4;  // All
		BottomPicker.SelectedIndex = 4; // All
		UpdateSafeAreaSettings();
	}

	private SafeAreaRegions GetSafeAreaRegionsFromPickerIndex(int index)
	{
		return index switch
		{
			0 => SafeAreaRegions.None,
			1 => SafeAreaRegions.SoftInput,
			2 => SafeAreaRegions.Container,
			3 => SafeAreaRegions.Default,
			4 => SafeAreaRegions.All,
			_ => SafeAreaRegions.None
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

		// Apply SafeAreaEdges to the ContentView
		TestContentView.SafeAreaEdges = safeAreaSettings;

		// Update the display label
		var settingsText = $"Left: {GetSafeAreaRegionsFromPickerIndex(LeftPicker.SelectedIndex)}, " +
						  $"Top: {GetSafeAreaRegionsFromPickerIndex(TopPicker.SelectedIndex)}, " +
						  $"Right: {GetSafeAreaRegionsFromPickerIndex(RightPicker.SelectedIndex)}, " +
						  $"Bottom: {GetSafeAreaRegionsFromPickerIndex(BottomPicker.SelectedIndex)}";

		CurrentSettingsLabel.Text = $"Current ContentView SafeAreaEdges: {settingsText}";
	}
}