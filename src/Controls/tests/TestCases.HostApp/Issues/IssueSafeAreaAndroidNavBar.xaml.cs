namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 0, "Test content flowing behind navigation bar on Android when SafeAreaEdges=None", PlatformAffected.Android)]
public partial class IssueSafeAreaAndroidNavBar : ContentPage
{
	public IssueSafeAreaAndroidNavBar()
	{
		InitializeComponent();
		UpdateSafeAreaLabel();
	}

	private void OnSetSafeAreaNoneClicked(object sender, EventArgs e)
	{
		this.SafeAreaEdges = SafeAreaEdges.None;
		UpdateSafeAreaLabel();
		TestStatusLabel.Text = "Test Status: SafeAreaEdges set to None - content should flow behind nav bar";
	}

	private void OnSetSafeAreaAllClicked(object sender, EventArgs e)
	{
		this.SafeAreaEdges = SafeAreaEdges.All;
		UpdateSafeAreaLabel();
		TestStatusLabel.Text = "Test Status: SafeAreaEdges set to All - content should respect safe area";
	}

	private void OnSetTopNoneBottomAllClicked(object sender, EventArgs e)
	{
		this.SafeAreaEdges = new SafeAreaEdges(
			SafeAreaRegions.All,    // Left
			SafeAreaRegions.None,   // Top
			SafeAreaRegions.All,    // Right
			SafeAreaRegions.All     // Bottom
		);
		UpdateSafeAreaLabel();
		TestStatusLabel.Text = "Test Status: Top=None, Bottom=All - content flows behind top nav bar only";
	}

	private void UpdateSafeAreaLabel()
	{
		var edges = this.SafeAreaEdges;
		CurrentSafeAreaLabel.Text = $"Current SafeAreaEdges: L={edges.Left}, T={edges.Top}, R={edges.Right}, B={edges.Bottom}";
	}
}
