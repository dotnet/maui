namespace Maui.Controls.Sample;

public partial class DatePickerOptionsPage : ContentPage
{
	private DatePickerViewModal _viewModel;
	public DatePickerOptionsPage(DatePickerViewModal viewModel)
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
		if (DateTime.TryParse(Date.Text, out var parsedDate))
		{
			_viewModel.Date = parsedDate;
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
	private void OnTextColorRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.TextColor = radioButton.Content.ToString() == "Red" ? Colors.Red : Colors.Green;
		}
	}
	private void SetMaximumDateButton_Clicked(object sender, EventArgs e)
	{
		if (DateTime.TryParse(MaximumDate.Text, out var parsedDate))
		{
			_viewModel.MaximumDate = parsedDate;
		}
	}
	private void SetMinimumDateButton_Clicked(object sender, EventArgs e)
	{
		if (DateTime.TryParse(MinimumDate.Text, out var parsedDate))
		{
			_viewModel.MinimumDate = parsedDate;
		}
	}
}