namespace Maui.Controls.Sample;

public partial class StackLayoutControlPage : NavigationPage
{
	private StackLayoutViewModel _viewModel;

	public StackLayoutControlPage()
	{
		_viewModel = new StackLayoutViewModel();
		PushAsync(new StackLayoutControlMainPage(_viewModel));
	}
}
public partial class StackLayoutControlMainPage : ContentPage
{
	private StackLayoutViewModel _viewModel;

	public StackLayoutControlMainPage(StackLayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new StackLayoutViewModel();
		await Navigation.PushAsync(new StackLayoutOptionsPage(_viewModel));
	}
}
