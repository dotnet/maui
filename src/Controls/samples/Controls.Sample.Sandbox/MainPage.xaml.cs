
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		//this.Loaded += MainPage_Loaded;
	}

	private async void MainPage_Loaded(object? sender, EventArgs e)
	{
		await Task.Delay(1000);

		await DisplayAlert("what", "what", "what");
		await Task.Delay(5000);

		await Navigation.PushAsync(new ContentPage());
	}
}
