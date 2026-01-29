namespace Maui.Controls.Sample;

public class TriggersControlPage : NavigationPage
{
	private TriggersViewModel _viewModel;

	public TriggersControlPage()
	{
		_viewModel = new TriggersViewModel();
		PushAsync(new TriggersControlMainPage(_viewModel));
	}
}

public partial class TriggersControlMainPage : ContentPage
{
	private TriggersViewModel _viewModel;

	public TriggersControlMainPage(TriggersViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

		// Display platform information
		UpdatePlatformLabel();
		// Display orientation information
		UpdateOrientationLabel();

		// Subscribe to window size changes for adaptive trigger testing
		this.SizeChanged += OnPageSizeChanged;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Update orientation when page appears
		UpdateOrientationLabel();

		// Subscribe to orientation changes
		DeviceDisplay.Current.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		// Unsubscribe from orientation changes
		DeviceDisplay.Current.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;

		// Unsubscribe from size changes
		this.SizeChanged -= OnPageSizeChanged;
	}

	private void OnPageSizeChanged(object sender, EventArgs e)
	{
		// Update window size label for adaptive trigger demo
		windowSizeLabel.Text = $"Window size: {Width:F0} x {Height:F0} (Width threshold: 800)";
	}

	private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
	{
		// Update orientation label when device orientation changes
		UpdateOrientationLabel();
	}

	private void UpdatePlatformLabel()
	{
		platformLabel.Text = $"Running on: {DeviceInfo.Platform}";
	}

	private void UpdateOrientationLabel()
	{
		var orientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
		orientationLabel.Text = $"Current orientation: {orientation}";
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new TriggersOptionsPage(_viewModel));
	}
}