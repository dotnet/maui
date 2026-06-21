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
		ClickedEventLabel.Text = string.Empty;
		PressedEventLabel.Text = string.Empty;
		ReleasedEventLabel.Text = string.Empty;
		await Navigation.PushAsync(new ButtonOptionsPage(_viewModel));
	}

	public void OnButtonClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button != null)
		{
			ClickedEventLabel.Text = "Clicked Event Executed";
		}
	}

	public void OnButtonPressed(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button != null)
		{
			PressedEventLabel.Text = "Pressed Event Executed";
		}
	}

	public void OnButtonReleased(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button != null)
		{
			ReleasedEventLabel.Text = "Released Event Executed";
		}
	}
}