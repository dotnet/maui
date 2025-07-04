namespace Maui.Controls.Sample;

public class PickerControlPage : NavigationPage
{
	private PickerViewModel _viewModel;

	public PickerControlPage()
	{
		_viewModel = new PickerViewModel();
#if ANDROID
			BarTextColor = Colors.White;
#endif
		PushAsync(new PickerControlMainPage(_viewModel));
	}
}

public partial class PickerControlMainPage : ContentPage
{
	private PickerViewModel _viewModel;

	public PickerControlMainPage(PickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new PickerViewModel();
		await Navigation.PushAsync(new PickerOptionsPage(_viewModel));
	}

	private void Picker_SelectedIndexChanged(object sender, EventArgs e)
	{
		SelectedIndexChangedStatusLabel.Text = "Triggered";
	}
}