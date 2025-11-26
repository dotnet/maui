using Maui.Controls.Sample;

namespace Maui.Controls.Sample;

public partial class SwitchOptionsPage : ContentPage
{
	private SwitchViewModel _viewModel;
	public SwitchOptionsPage(SwitchViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}
	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnFlowDirectionCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is CheckBox changedBox) || !changedBox.IsChecked)
			return;
		if (changedBox == FlowDirectionLeftToRightCheckBox)
		{
			FlowDirectionRightToLeftCheckBox.IsChecked = false;
			_viewModel.FlowDirection = FlowDirection.LeftToRight;
		}
		else if (changedBox == FlowDirectionRightToLeftCheckBox)
		{
			FlowDirectionLeftToRightCheckBox.IsChecked = false;
			_viewModel.FlowDirection = FlowDirection.RightToLeft;
		}
	}

	private void OnEnabledCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			_viewModel.IsEnabled = false;
		}
		else
		{
			_viewModel.IsEnabled = true;
		}
	}

	private void OnVisibleCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{

		if (e.Value)
		{
			_viewModel.IsVisible = false;
		}
		else
		{
			_viewModel.IsVisible = true;
		}
	}

	private void OnToggledCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			_viewModel.IsToggled = true;
		}
		else
		{
			_viewModel.IsToggled = false;
		}
	}

	private void OnOnColorCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is CheckBox changedBox) || !changedBox.IsChecked)
			return;
		if (changedBox == OnColorRedCheckBox)
		{
			OnColorGreenCheckBox.IsChecked = false;
			_viewModel.OnColor = Colors.Red;
		}
		else if (changedBox == OnColorGreenCheckBox)
		{
			OnColorRedCheckBox.IsChecked = false;
			_viewModel.OnColor = Colors.Green;
		}
	}

	private void OnShadowCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			_viewModel.Shadow = new Shadow { Brush = Colors.Violet, Radius = 20, Offset = new Point(0, 0), Opacity = 1f };
		}
		else
		{
			_viewModel.Shadow = null;
		}
	}

	private void OnThumbColorCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is CheckBox changedBox) || !changedBox.IsChecked)
			return;
		if (changedBox == ThumbColorRedCheckBox)
		{
			ThumbColorGreenCheckBox.IsChecked = false;
			_viewModel.ThumbColor = Colors.Red;
		}
		else if (changedBox == ThumbColorGreenCheckBox)
		{
			ThumbColorRedCheckBox.IsChecked = false;
			_viewModel.ThumbColor = Colors.Green;
		}
	}
}