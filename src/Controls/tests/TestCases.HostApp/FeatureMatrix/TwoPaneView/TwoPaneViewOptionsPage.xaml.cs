namespace Maui.Controls.Sample;

public partial class TwoPaneViewOptionsPage : ContentPage
{
	private TwoPaneViewViewModel _viewModel;

	public TwoPaneViewOptionsPage(TwoPaneViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		TallModeStepper.Value = _viewModel.MinTallModeHeight;
		WideModeStepper.Value = _viewModel.MinWideModeWidth;
		Pane1LengthStepper.Value = ParseLength(_viewModel.Pane1Length);
		Pane2LengthStepper.Value = ParseLength(_viewModel.Pane2Length);

		TallModeHeightValue.Text = $"{_viewModel.MinTallModeHeight}px";
		WideModeWidthValue.Text = $"{_viewModel.MinWideModeWidth}px";
		Pane1LengthValue.Text = _viewModel.Pane1Length.ToString();
		Pane2LengthValue.Text = _viewModel.Pane2Length.ToString();
	}

	private void OnFlowDirectionCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			_viewModel.FlowDirection = FlowDirection.RightToLeft;
		}
		else
		{
			_viewModel.FlowDirection = FlowDirection.LeftToRight;
		}
	}

	private void OnShadowCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.IsShadowEnabled = e.Value;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
	private void OnTallModeHeightChanged(object sender, ValueChangedEventArgs e)
	{
		var value = (int)e.NewValue;
		_viewModel.MinTallModeHeight = value;
		TallModeHeightValue.Text = $"{value}px";
	}

	private void OnWideModeWidthChanged(object sender, ValueChangedEventArgs e)
	{
		var value = (int)e.NewValue;
		_viewModel.MinWideModeWidth = value;
		WideModeWidthValue.Text = $"{value}px";
	}

	private void OnPane1LengthChanged(object sender, ValueChangedEventArgs e)
	{
		var value = e.NewValue;
		_viewModel.Pane1Length = new GridLength(value, GridUnitType.Star);
		Pane1LengthValue.Text = $"{value}";
	}

	private void OnPane2LengthChanged(object sender, ValueChangedEventArgs e)
	{
		var value = e.NewValue;
		_viewModel.Pane2Length = new GridLength(value, GridUnitType.Star);
		Pane2LengthValue.Text = $"{value}";
	}

	private double ParseLength(GridLength length)
	{
		return length.IsStar ? length.Value : 1; // Default to 1* if not a star value
	}
}