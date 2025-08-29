namespace Maui.Controls.Sample;

public partial class ContentViewOptionsPage : ContentPage
{
	private ContentViewViewModel _viewModel;

	public ContentViewOptionsPage(ContentViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;

		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Apply",
			Command = new Command(() =>
			{
				Navigation.PopAsync();
			})
		});
	}

	private void OnCustomPageRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (FirstPageRadioButton.IsChecked)
		{
			_viewModel.ContentLabel = "FirstCustomPage";
		}
		else if (SecondPageRadioButton.IsChecked)
		{
			_viewModel.ContentLabel = "SecondCustomPage";
		}
		else if (DefaultPageRadioButton.IsChecked)
		{
			_viewModel.ContentLabel = "DefaultPage";
		}
	}

	private void OnControlTemplateRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (ControlTemplateYesRadioButton.IsChecked)
		{
			_viewModel.UseControlTemplate = true;
			if (_viewModel.ContentLabel == "FirstCustomPage")
			{
				_viewModel.ControlTemplateKeyFirst = "AlternateCardTemplate";
			}
			else if (_viewModel.ContentLabel == "SecondCustomPage")
			{
				_viewModel.ControlTemplateKeySecond = "AlternateSecondTemplate";
			}
		}
		else if (ControlTemplateNoRadioButton.IsChecked)
		{
			_viewModel.UseControlTemplate = false;
			if (_viewModel.ContentLabel == "FirstCustomPage")
			{
				_viewModel.ControlTemplateKeyFirst = "DefaultFirstTemplate";
			}
			else if (_viewModel.ContentLabel == "SecondCustomPage")
			{
				_viewModel.ControlTemplateKeySecond = "DefaultSecondTemplate";
			}
		}
	}


	private void OnHeightRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (Height50RadioButton.IsChecked)
			_viewModel.HeightRequest = 50;
		else if (Height300RadioButton.IsChecked)
			_viewModel.HeightRequest = 300;
	}

	private void OnWidthRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (Width50RadioButton.IsChecked)
			_viewModel.WidthRequest = 50;
		else if (Width400RadioButton.IsChecked)
			_viewModel.WidthRequest = 400;
	}

	private void OnBgColorRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (BgLightGreenRadioButton.IsChecked)
			_viewModel.BackgroundColor = Colors.LightGreen;
		else if (BgSkyBlueRadioButton.IsChecked)
			_viewModel.BackgroundColor = Colors.Blue;
	}

	private void OnEnabledRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (EnabledTrueRadioButton.IsChecked)
			_viewModel.IsEnabled = true;
		else if (EnabledFalseRadioButton.IsChecked)
			_viewModel.IsEnabled = false;
	}

	private void OnVisibleRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (VisibleTrueRadioButton.IsChecked)
			_viewModel.IsVisible = true;
		else if (VisibleFalseRadioButton.IsChecked)
			_viewModel.IsVisible = false;
	}

	private void FlowDirection_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender == FlowDirectionLeftToRight)
		{
			_viewModel.FlowDirection = FlowDirection.LeftToRight;
		}
		else if (sender == FlowDirectionRightToLeft)
		{
			_viewModel.FlowDirection = FlowDirection.RightToLeft;
		}
	}
}