namespace Maui.Controls.Sample;

public class ButtonControlPage : NavigationPage
{
	public ButtonControlPage()
	{
		PushAsync(new ButtonControlMainPage());
	}
}

public partial class ButtonControlMainPage : ContentPage
{
	public ButtonViewModal _viewModel;
	public ButtonControlMainPage()
	{
		InitializeComponent();
		BindingContext = _viewModel = new ButtonViewModal();
	}

	public async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new ButtonViewModal();
		await Navigation.PushAsync(new ButtonOptionsPage(_viewModel));
	}
}