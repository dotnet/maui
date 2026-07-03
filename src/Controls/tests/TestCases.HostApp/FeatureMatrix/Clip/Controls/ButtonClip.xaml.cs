namespace Maui.Controls.Sample;

public partial class ButtonClip : ContentPage
{
	private ClipViewModel _viewModel;
	public ButtonClip()
	{
		InitializeComponent();
		_viewModel = new ClipViewModel();
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new ClipViewModel();
		await Navigation.PushAsync(new ClipOptionsPage(_viewModel));
	}
}