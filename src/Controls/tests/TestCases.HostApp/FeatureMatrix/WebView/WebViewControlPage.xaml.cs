using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public class WebViewControlPage : NavigationPage
{
	private WebViewViewModel _viewModel;
	public WebViewControlPage()
	{
		_viewModel = new WebViewViewModel();
		BindingContext = _viewModel;
		PushAsync(new WebViewControlMainPage(_viewModel));
	}
}
public partial class WebViewControlMainPage : ContentPage
{
	private WebViewViewModel _viewModel;
	public WebViewControlMainPage(WebViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		_viewModel.WebViewReference = WebViewControl;
	}
	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		var oldViewModel = _viewModel;
		_viewModel = new WebViewViewModel();
		_viewModel.CopyWebViewStateFrom(oldViewModel);
		BindingContext = _viewModel;
		await Navigation.PushAsync(new WebViewOptionsPage(_viewModel));
	}
	private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
	{
		_viewModel.OnNavigating(sender, e);
	}
	private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
	{
		_viewModel.OnNavigated(sender, e);
	}
	private void OnWebViewProcessTerminated(object sender, EventArgs e)
	{
		_viewModel.OnProcessTerminated(sender, e);
	}
}