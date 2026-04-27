namespace Maui.Controls.Sample;

public class BorderControlPage : NavigationPage
{
	public BorderControlPage()
	{
		var viewModel = new BorderViewModel();
		PushAsync(new BorderControlMainPage(viewModel));
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
		_viewModel.Reset();
		await Navigation.PushAsync(new OptionsPage(_viewModel));
	}
}
