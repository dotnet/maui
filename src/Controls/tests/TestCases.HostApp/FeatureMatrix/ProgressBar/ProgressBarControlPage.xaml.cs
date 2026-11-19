namespace Maui.Controls.Sample;

public partial class ProgressBarControlPage : ContentPage
{
	readonly ProgressBarViewModel _viewModel;

	public ProgressBarControlPage()
	{
		InitializeComponent();
		BindingContext = _viewModel = new ProgressBarViewModel();
	}

	void ProgressToButton_Clicked(object sender, EventArgs e)
	{
		if (_viewModel.TryGetProgressToValue(out double targetProgress))
		{
			progressBarControl.ProgressTo(targetProgress, 1000, Easing.Linear);
		}
	}

	void ResetButton_Clicked(object sender, EventArgs e)
	{
		// ProgressTo sets Progress as a local value, which overrides the binding; clear it so the binding applies again.
		progressBarControl.AbortAnimation("Progress");
		progressBarControl.ClearValue(ProgressBar.ProgressProperty);
		_viewModel.Reset();
	}
}
