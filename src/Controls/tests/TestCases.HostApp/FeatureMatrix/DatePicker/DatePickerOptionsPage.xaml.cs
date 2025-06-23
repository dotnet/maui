using System.Globalization;

namespace Maui.Controls.Sample;

public partial class DatePickerOptionsPage : ContentPage
{
	private DatePickerViewModel _viewModel;
	public DatePickerOptionsPage(DatePickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}
	protected override void OnAppearing()
	{
		base.OnAppearing();
	}
	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void SetDateButton_Clicked(object sender, EventArgs e)
	{
		if (DateTime.TryParseExact(Date.Text, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
		{
			_viewModel.Date = parsedDate;
		}
	}

	private void OnFlowDirectionRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
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

	private void OnIsEnabledRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsEnabled = false;
		}
	}

	private void OnIsVisibleRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsVisible = false;
		}
	}

	private void OnShadowRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.Shadow = new Shadow { Brush = Colors.Violet, Radius = 20, Offset = new Point(0, 0), Opacity = 1f };
		}
	}
	private void OnTextColorButtonClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button != null)
		{
			_viewModel.TextColor = button.Text == "Red" ? Colors.Red : Colors.Green;
		}
	}

	private void SetFormatButton_Clicked(object sender, EventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(Format.Text))
		{
			_viewModel.Format = Format.Text;
		}
	}
	private void SetMaximumDateButton_Clicked(object sender, EventArgs e)
	{
		if (DateTime.TryParseExact(MaximumDate.Text, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
		{
			_viewModel.MaximumDate = parsedDate;
		}
	}
	private void SetMinimumDateButton_Clicked(object sender, EventArgs e)
	{
		if (DateTime.TryParseExact(MinimumDate.Text, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedDate))
		{
			_viewModel.MinimumDate = parsedDate;
		}
	}

	private void OnCultureButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			string cultureName = button.Text;
			try
			{
				CultureInfo culture = new CultureInfo(cultureName);
				_viewModel.Culture = culture;
			}
			catch (Exception ex)
			{
				DisplayAlert("Culture Error", $"Failed to set culture {cultureName}: {ex.Message}", "OK");
			}
		}
	}
}