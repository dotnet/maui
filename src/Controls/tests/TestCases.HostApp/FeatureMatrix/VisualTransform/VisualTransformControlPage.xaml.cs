namespace Maui.Controls.Sample;

public partial class VisualTransformControlPage : NavigationPage
{

	private VisualTransformViewModal _viewModel;

	public VisualTransformControlPage()
	{
		_viewModel = new VisualTransformViewModal();
		PushAsync(new VisualTransformControlMainPage(_viewModel));
	}
}

public partial class VisualTransformControlMainPage : ContentPage
{
	private VisualTransformViewModal _viewModel;

	public VisualTransformControlMainPage(VisualTransformViewModal viewModel)
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