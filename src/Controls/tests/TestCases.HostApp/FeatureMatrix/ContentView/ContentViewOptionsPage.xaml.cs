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
	}

	private void OnControlTemplateRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (ControlTemplateYesRadioButton.IsChecked)
			_viewModel.UseControlTemplate = true;
		else if (ControlTemplateNoRadioButton.IsChecked)
			_viewModel.UseControlTemplate = false;
	}
	
	private void OnHeightRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (Height150RadioButton.IsChecked)
			_viewModel.HeightRequest = 150;
		else if (Height300RadioButton.IsChecked)
			_viewModel.HeightRequest = 300;
	}

	private void OnWidthRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (Width200RadioButton.IsChecked)
			_viewModel.WidthRequest = 200;
		else if (Width400RadioButton.IsChecked)
			_viewModel.WidthRequest = 400;
	}

	private void OnBgColorRadioCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (BgLightGreenRadioButton.IsChecked)
			_viewModel.BackgroundColor = Colors.LightGreen;
		else if (BgSkyBlueRadioButton.IsChecked)
			_viewModel.BackgroundColor = Colors.SkyBlue;
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
}