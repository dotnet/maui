#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea attached property for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 0)]
public partial class Issue28986 : ContentPage
{
	public Issue28986()
	{
		InitializeComponent();
		UpdateCurrentSettingsLabel();

#if ANDROID
		// Set SoftInput.AdjustNothing - we have full control over insets (iOS-like behavior)
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}

	private void OnGridSetNoneClicked(object sender, EventArgs e)
	{
		MainGrid.SafeAreaEdges = SafeAreaEdges.None;
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetContainerClicked(object sender, EventArgs e)
	{
		MainGrid.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetAllClicked(object sender, EventArgs e)
	{
		MainGrid.SafeAreaEdges = SafeAreaEdges.All;
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetBottomSoftInputClicked(object sender, EventArgs e)
	{
		var current = MainGrid.SafeAreaEdges;
		MainGrid.SafeAreaEdges = new SafeAreaEdges(
			current.Left,     // Left
			current.Top,     // Top
			current.Right,   // Right
			SafeAreaRegions.SoftInput       // Bottom
		);
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetTopContainerClicked(object sender, EventArgs e)
	{
		var current = MainGrid.SafeAreaEdges;
		MainGrid.SafeAreaEdges = new SafeAreaEdges(
			current.Left,              // Left (keep current)
			SafeAreaRegions.Container, // Top
			current.Right,             // Right (keep current)
			current.Bottom             // Bottom (keep current)
		);
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetTopNoneClicked(object sender, EventArgs e)
	{
		var current = MainGrid.SafeAreaEdges;
		MainGrid.SafeAreaEdges = new SafeAreaEdges(
			current.Left,         // Left (keep current)
			SafeAreaRegions.None, // Top
			current.Right,        // Right (keep current)
			current.Bottom        // Bottom (keep current)
		);
		UpdateCurrentSettingsLabel();
	}

	private void UpdateCurrentSettingsLabel()
	{
		var edges = MainGrid.SafeAreaEdges;
		var settingText = GetSafeAreaEdgesDescription(edges);
		CurrentSettingsLabel.Text = $"Current MainGrid SafeAreaEdges: {settingText}";
	}

	private string GetSafeAreaEdgesDescription(SafeAreaEdges edges)
	{
		// Check for common patterns
		if (edges.Left == SafeAreaRegions.None && edges.Top == SafeAreaRegions.None &&
			edges.Right == SafeAreaRegions.None && edges.Bottom == SafeAreaRegions.None)
		{
			return "None (Edge-to-edge)";
		}

		if (edges.Left == SafeAreaRegions.All && edges.Top == SafeAreaRegions.All &&
			edges.Right == SafeAreaRegions.All && edges.Bottom == SafeAreaRegions.All)
		{
			return "All (Full safe area)";
		}

		if (edges.Left == SafeAreaRegions.Container && edges.Top == SafeAreaRegions.Container &&
			edges.Right == SafeAreaRegions.Container && edges.Bottom == SafeAreaRegions.Container)
		{
			return "Container (Respect notches/bars)";
		}

		// For mixed values, show individual edges
		return $"Left:{edges.Left}, Top:{edges.Top}, Right:{edges.Right}, Bottom:{edges.Bottom}";
	}
}