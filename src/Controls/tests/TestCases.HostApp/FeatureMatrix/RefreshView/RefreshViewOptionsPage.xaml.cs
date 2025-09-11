namespace Maui.Controls.Sample;

public partial class RefreshViewOptionsPage : ContentPage
{
	private RefreshViewViewModel _viewModel;

	public RefreshViewOptionsPage(RefreshViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.CommandParameter = button.Text;
		}
	}

	private void OnFlowDirectionRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}
	}
	private void OnIsEnabledRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.IsEnabled = false;
		}
	}

	private void OnIsVisibleRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.IsVisible = false;
		}
	}

	private void OnShadowRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.Shadow = new Shadow { Brush = Colors.Violet, Radius = 20, Offset = new Point(0, 0), Opacity = 1f };
		}
	}

	private void OnIsRefreshingRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.Continue = true;
			_viewModel.IsRefreshing = true;
		}
	}

	private void OnRefreshColorRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && radioButton.IsChecked)
		{
			switch (radioButton.Content.ToString())
			{
				case "Red":
					_viewModel.RefreshColor = Colors.Red;
					break;
				case "Blue":
					_viewModel.RefreshColor = Colors.Blue;
					break;
				case "Green":
					_viewModel.RefreshColor = Colors.Green;
					break;
				case "Orange":
					_viewModel.RefreshColor = Colors.Orange;
					break;
				default:
					_viewModel.RefreshColor = null;
					break;
			}
		}
	}
}