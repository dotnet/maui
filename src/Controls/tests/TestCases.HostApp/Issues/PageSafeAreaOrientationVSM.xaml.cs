namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Page SafeAreaEdges with OrientationStateTrigger VSM Test", PlatformAffected.All)]
public partial class PageSafeAreaOrientationVSM : ContentPage
{
	public PageSafeAreaOrientationVSM()
	{
		InitializeComponent();

		// Monitor size changes for orientation detection
		this.SizeChanged += OnSizeChanged;

		// Update info when page appears
		this.Appearing += OnPageAppearing;
	}

	private void OnPageAppearing(object sender, EventArgs e)
	{
		// Delay to allow layout to complete
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), UpdateInfo);
	}

	private void OnSizeChanged(object sender, EventArgs e)
	{
		// Update info when size changes (indicating orientation change)
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), UpdateInfo);
	}

	private void UpdateInfo()
	{
		try
		{
			// Determine orientation based on actual page dimensions
			var isLandscape = this.Width > this.Height;
			var orientationText = isLandscape ? "Landscape" : "Portrait";

			OrientationLabel.Text = $"Orientation: {orientationText}";
			SafeAreaEdgesLabel.Text = $"SafeAreaEdges: {this.SafeAreaEdges}";
			PageDimensionsLabel.Text = $"Page Dimensions: {this.Width:F1} x {this.Height:F1}";
		}
		catch (Exception ex)
		{
			OrientationLabel.Text = $"Orientation: Error - {ex.Message}";
			SafeAreaEdgesLabel.Text = "SafeAreaEdges: Error";
			PageDimensionsLabel.Text = "Page Dimensions: Error";
		}
	}
}
