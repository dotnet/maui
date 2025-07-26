namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30337, "Implement SafeArea attached property for per-edge safe area control",
	PlatformAffected.All)]
public partial class Issue30337 : ContentPage
{
	public Issue30337()
	{
		InitializeComponent();
	}

	private void OnLayoutAllClicked(object sender, EventArgs e)
	{
		StatusLabel.Text = "Layout SafeAreaEdges.All clicked - would obey all safe area insets when feature is implemented";
	}

	private void OnContentViewNoneClicked(object sender, EventArgs e)
	{
		StatusLabel.Text = "ContentView SafeAreaEdges.None clicked - would extend edge-to-edge when feature is implemented";
	}

	private void OnBorderMixedClicked(object sender, EventArgs e)
	{
		StatusLabel.Text = "Border mixed edges clicked - would implement Left=All, Top=None, Right=All, Bottom=None when feature is implemented";
	}

	private void OnGridContainerClicked(object sender, EventArgs e)
	{
		StatusLabel.Text = "Grid SafeAreaEdges.Container clicked - would flow under keyboard, stay out of bars/notch when feature is implemented";
	}

	private void OnScrollViewSoftInputClicked(object sender, EventArgs e)
	{
		StatusLabel.Text = "ScrollView SafeAreaEdges.SoftInput clicked - would always pad to avoid keyboard when feature is implemented";
	}

	private void OnSetNoneClicked(object sender, EventArgs e)
	{
		// TODO: When SafeAreaEdges is implemented, this would set DynamicLayout.SafeAreaEdges = SafeAreaEdges.None;
		DynamicLabel.Text = "Would set: SafeAreaEdges.None - edge-to-edge";
		StatusLabel.Text = "Dynamic property would be set to None when feature is implemented";
	}

	private void OnSetAllClicked(object sender, EventArgs e)
	{
		// TODO: When SafeAreaEdges is implemented, this would set DynamicLayout.SafeAreaEdges = SafeAreaEdges.All;
		DynamicLabel.Text = "Would set: SafeAreaEdges.All - obey all safe areas";
		StatusLabel.Text = "Dynamic property would be set to All when feature is implemented";
	}

	private void OnSetContainerClicked(object sender, EventArgs e)
	{
		// TODO: When SafeAreaEdges is implemented, this would set DynamicLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		DynamicLabel.Text = "Would set: SafeAreaEdges.Container - flows under keyboard";
		StatusLabel.Text = "Dynamic property would be set to Container when feature is implemented";
	}

	private void OnSetSoftInputClicked(object sender, EventArgs e)
	{
		// TODO: When SafeAreaEdges is implemented, this would set DynamicLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
		DynamicLabel.Text = "Would set: SafeAreaEdges.SoftInput - always pads for keyboard";
		StatusLabel.Text = "Dynamic property would be set to SoftInput when feature is implemented";
	}
}