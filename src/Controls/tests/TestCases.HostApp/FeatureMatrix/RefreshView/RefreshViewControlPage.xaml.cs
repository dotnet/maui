namespace Maui.Controls.Sample;

public class RefreshViewControlPage : NavigationPage
{
	private RefreshViewViewModel _viewModel;

	public RefreshViewControlPage()
	{
		_viewModel = new RefreshViewViewModel();
#if ANDROID
			BarTextColor = Colors.White;
#endif
		PushAsync(new RefreshViewControlMainPage(_viewModel));
	}
}

public partial class RefreshViewControlMainPage : ContentPage
{
	private RefreshViewViewModel _viewModel;

	public RefreshViewControlMainPage(RefreshViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new RefreshViewViewModel();
		await Navigation.PushAsync(new RefreshViewOptionsPage(_viewModel));
	}
}