namespace Maui.Controls.Sample;

public partial class TimePickerOptionsPage : ContentPage
{
	private TimePickerViewModel _viewModel;
	public TimePickerOptionsPage(TimePickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnFlowDirectionRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}
	}

	private void OnFontAttributesRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FontAttributes = radioButton.Content.ToString() == "Italic" ? FontAttributes.Italic : radioButton.Content.ToString() == "Bold" ? FontAttributes.Bold : FontAttributes.None;
		}
	}

	private void OnFontFamilyRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FontFamily = radioButton.Content.ToString() == "Dokdo" ? "Dokdo" : "MontserratBold";
		}
	}

	public void SetFormatButton_Clicked(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(Format.Text))
		{
			_viewModel.Format = Format.Text;
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

	public void SetTimeButton_Clicked(object sender, EventArgs e)
	{
		if (TimeSpan.TryParse(Time.Text, out TimeSpan time))
		{
			_viewModel.Time = time;
		}
	}

	private void OnTextColorRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.TextColor = radioButton.Content.ToString() == "Red" ? Colors.Red : Colors.Green;
		}
	}

	private void OnCultureButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			string cultureName = button.Text;
			try
			{
				var culture = new System.Globalization.CultureInfo(cultureName);
				_viewModel.Culture = culture;
			}
			catch (Exception ex)
			{
				DisplayAlert("Culture Error", $"Failed to set culture {cultureName}: {ex.Message}", "OK");
			}
		}
	}
}