using Android.Views;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea ScrollView for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 4)]
public partial class Issue28986_ScrollView : ContentPage
{
	public Issue28986_ScrollView()
	{
		InitializeComponent();
		UpdateCurrentSettingsLabel();

#if ANDROID
		// Set SoftInput.AdjustNothing - we have full control over insets (iOS-like behavior)
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}

	private void OnScrollViewSetNoneClicked(object sender, EventArgs e)
	{
		TestScrollView.SafeAreaEdges = SafeAreaEdges.None;
		UpdateCurrentSettingsLabel();
	}

	private void OnScrollViewSetContainerClicked(object sender, EventArgs e)
	{
		TestScrollView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		UpdateCurrentSettingsLabel();
	}

	private void OnScrollViewSetAllClicked(object sender, EventArgs e)
	{
		TestScrollView.SafeAreaEdges = SafeAreaEdges.All;
		UpdateCurrentSettingsLabel();
	}

	private void OnScrollViewSetSoftInputClicked(object sender, EventArgs e)
	{
		TestScrollView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
		UpdateCurrentSettingsLabel();
	}

	private void UpdateCurrentSettingsLabel()
	{
		var edges = TestScrollView.SafeAreaEdges;
		var settingText = GetSafeAreaEdgesDescription(edges);
		CurrentSettingsLabel.Text = $"Current ScrollView SafeAreaEdges: {settingText}";
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

		if (edges.Left == SafeAreaRegions.SoftInput && edges.Top == SafeAreaRegions.SoftInput &&
			edges.Right == SafeAreaRegions.SoftInput && edges.Bottom == SafeAreaRegions.SoftInput)
		{
			return "SoftInput (Avoid keyboard only)";
		}

		// For mixed values, show individual edges
		return $"Left:{edges.Left}, Top:{edges.Top}, Right:{edges.Right}, Bottom:{edges.Bottom}";
	}
}