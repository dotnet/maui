namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea ContentPage for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 2)]
public partial class Issue28986_ContentPage : ContentPage
{
	public Issue28986_ContentPage()
	{
		InitializeComponent();
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetNoneClicked(object sender, EventArgs e)
	{
		TestContentPage.SafeAreaEdges = SafeAreaEdges.None;
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetContainerClicked(object sender, EventArgs e)
	{
		TestContentPage.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetAllClicked(object sender, EventArgs e)
	{
		TestContentPage.SafeAreaEdges = SafeAreaEdges.All;
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetBottomSoftInputClicked(object sender, EventArgs e)
	{
		var current = TestContentPage.SafeAreaEdges;
		TestContentPage.SafeAreaEdges = new SafeAreaEdges(
			current.Left,     // Left
			current.Top,     // Top
			current.Right,   // Right
			SafeAreaRegions.SoftInput       // Bottom
		);
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetTopContainerClicked(object sender, EventArgs e)
	{
		var current = TestContentPage.SafeAreaEdges;
		TestContentPage.SafeAreaEdges = new SafeAreaEdges(
			current.Left,              // Left (keep current)
			SafeAreaRegions.Container, // Top
			current.Right,             // Right (keep current)
			current.Bottom             // Bottom (keep current)
		);
		UpdateCurrentSettingsLabel();
	}

	private void OnGridSetTopNoneClicked(object sender, EventArgs e)
	{
		var current = TestContentPage.SafeAreaEdges;
		TestContentPage.SafeAreaEdges = new SafeAreaEdges(
			current.Left,         // Left (keep current)
			SafeAreaRegions.None, // Top
			current.Right,        // Right (keep current)
			current.Bottom        // Bottom (keep current)
		);
		UpdateCurrentSettingsLabel();
	}

	private void UpdateCurrentSettingsLabel()
	{
		var edges = TestContentPage.SafeAreaEdges;
		var settingText = GetSafeAreaEdgesDescription(edges);
		CurrentSettingsLabel.Text = $"Current TestContentPage SafeAreaEdges: {settingText}";
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