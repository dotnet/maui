namespace Maui.Controls.Sample;

public partial class VisualTransformControlPage : NavigationPage
{

	private VisualTransformViewModel _viewModel;

	public VisualTransformControlPage()
	{
		_viewModel = new VisualTransformViewModel();
		PushAsync(new VisualTransformControlMainPage(_viewModel));
	}
}

public partial class VisualTransformControlMainPage : ContentPage
{
	private VisualTransformViewModel _viewModel;

	public VisualTransformControlMainPage(VisualTransformViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new VisualTransformOptionsPage(_viewModel));
	}
}