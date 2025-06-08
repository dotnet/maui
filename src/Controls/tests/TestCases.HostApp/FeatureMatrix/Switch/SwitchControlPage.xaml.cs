namespace Maui.Controls.Sample;

public class SwitchControlPage : NavigationPage
{
	private SwitchViewModel _viewModel;
	public SwitchControlPage()
	{
		_viewModel = new SwitchViewModel();
		PushAsync(new SwitchControlMainPage(_viewModel));
	}
}

public partial class SwitchControlMainPage : ContentPage
{
	private SwitchViewModel _viewModel;

	public SwitchControlMainPage(SwitchViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new SwitchViewModel();
		_viewModel.BackgroundColor = null;
		_viewModel.OnColor = null;
		_viewModel.ThumbColor = null;
		await Navigation.PushAsync(new SwitchOptionsPage(_viewModel));
	}
}