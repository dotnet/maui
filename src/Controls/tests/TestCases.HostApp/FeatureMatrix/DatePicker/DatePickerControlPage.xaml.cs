namespace Maui.Controls.Sample;

public class DatePickerControlPage : NavigationPage
{
	public DatePickerControlPage()
	{
		PushAsync(new DatePickerMainControlPage());
	}
}

public partial class DatePickerMainControlPage : ContentPage
{
	private DatePickerViewModal _viewModel;

	public DatePickerMainControlPage()
	{
		InitializeComponent();
		_viewModel = new DatePickerViewModal();
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new DatePickerViewModal();
		await Navigation.PushAsync(new DatePickerOptionsPage(_viewModel));
	}
}