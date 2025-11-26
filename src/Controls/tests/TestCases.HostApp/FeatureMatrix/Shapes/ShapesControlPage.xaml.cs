namespace Maui.Controls.Sample;

public class ShapesControlPage : NavigationPage
{
	private ShapesViewModel _viewModel;

	public ShapesControlPage()
	{
		_viewModel = new ShapesViewModel();
		PushAsync(new ShapesControlMainPage(_viewModel));
	}
}
public partial class ShapesControlMainPage : ContentPage
{
	private ShapesViewModel _viewModel;

	public ShapesControlMainPage(ShapesViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new ShapesViewModel();
		await Navigation.PushAsync(new ShapesOptionsPage(_viewModel));
	}
}