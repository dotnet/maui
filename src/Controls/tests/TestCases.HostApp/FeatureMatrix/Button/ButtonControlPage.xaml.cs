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
	public ButtonViewModel _viewModel;
	public ButtonControlMainPage()
	{
		InitializeComponent();
		BindingContext = _viewModel = new ButtonViewModel();
	}

	public async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		_viewModel.Reset();
		await Navigation.PushAsync(new ButtonOptionsPage(_viewModel));
	}

	public void OnButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button)
			_viewModel.ClickedEventLabelText = "Clicked Event Executed";
	}

	public void OnButtonPressed(object sender, EventArgs e)
	{
		if (sender is Button)
			_viewModel.PressedEventLabelText = "Pressed Event Executed";
	}

	public void OnButtonReleased(object sender, EventArgs e)
	{
		if (sender is Button)
			_viewModel.ReleasedEventLabelText = "Released Event Executed";
	}
}