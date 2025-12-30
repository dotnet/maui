namespace Maui.Controls.Sample;

public class BorderControlPage : NavigationPage
{
	private BorderViewModel _viewModel;

	public BorderControlPage()
	{
		_viewModel = new BorderViewModel();
		PushAsync(new BorderControlMainPage(_viewModel));
	}
}

public partial class BorderControlMainPage : ContentPage
{
	private BorderViewModel _viewModel;

	public BorderControlMainPage(BorderViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new BorderViewModel();
		await Navigation.PushAsync(new OptionsPage(_viewModel));
	}
}
