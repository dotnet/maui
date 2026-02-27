namespace Maui.Controls.Sample;

public partial class ProgressBarControlPage : ContentPage
{
	private ProgressBarViewModel _viewModel;
	private ProgressBar progressBar;

	public ProgressBarControlPage()
	{
		InitializeComponent();
		_viewModel = new ProgressBarViewModel();
		BindingContext = _viewModel;
	}

	private void ReinitializeProgressBar()
	{
		BindingContext = _viewModel = new ProgressBarViewModel();
		ProgressBarGrid.Children.Clear();
		progressBar = new ProgressBar
		{
			AutomationId = "ProgressBarControl",
		};

		progressBar.SetBinding(ProgressBar.BackgroundColorProperty, nameof(ProgressBarViewModel.BackgroundColor));
		progressBar.SetBinding(ProgressBar.FlowDirectionProperty, nameof(ProgressBarViewModel.FlowDirection));
		progressBar.SetBinding(ProgressBar.IsVisibleProperty, nameof(ProgressBarViewModel.IsVisible));
		progressBar.SetBinding(ProgressBar.ProgressProperty, nameof(ProgressBarViewModel.Progress));
		progressBar.SetBinding(ProgressBar.ProgressColorProperty, nameof(ProgressBarViewModel.ProgressColor));
		progressBar.SetBinding(ProgressBar.ShadowProperty, nameof(ProgressBarViewModel.Shadow));
		ProgressBarGrid.Children.Add(progressBar);
	}

	private void OnProgressChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(ProgressEntry.Text, out double progress))
		{
			_viewModel.Progress = progress;
		}
	}

	private void ProgressColorButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		if (button.Text == "Green")
			_viewModel.ProgressColor = Colors.Green;
		else if (button.Text == "Red")
			_viewModel.ProgressColor = Colors.Red;
	}

	private void BackgroundColorButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		if (button.Text == "Orange")
			_viewModel.BackgroundColor = Colors.Orange;
		else if (button.Text == "Light Blue")
			_viewModel.BackgroundColor = Colors.LightBlue;
	}

	private void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsVisible = false;
		}
	}

	private void OnFlowDirectionChanged(object sender, EventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}
	}

	private void OnShadowCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = (RadioButton)sender;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.Shadow = new Shadow { Brush = Colors.Violet, Radius = 20, Offset = new Point(0, 0), Opacity = 1f };
		}
	}

	private void ProgressToButton_Clicked(object sender, EventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(ProgressToEntry.Text))
		{
			if (progressBar == null)
			{
				progressBarControl.ProgressTo(double.Parse(ProgressToEntry.Text), 1000, Easing.Linear);
			}
			else
			{
				progressBar.ProgressTo(double.Parse(ProgressToEntry.Text), 1000, Easing.Linear);
			}
		}
	}

	private void ResetButton_Clicked(object sender, EventArgs e)
	{
		ProgressEntry.Text = "0.50";
		ProgressToEntry.Text = "0";
		IsVisibleTrueRadio.IsChecked = false;
		FlowDirectionLTR.IsChecked = false;
		FlowDirectionRTL.IsChecked = false;
		ShadowFalseRadio.IsChecked = false;
		ReinitializeProgressBar();
	}
}