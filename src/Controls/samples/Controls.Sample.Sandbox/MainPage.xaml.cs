namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		System.Diagnostics.Debug.WriteLine("[SafeArea] MainPage initialized");
	}

	private async void OnGoToSignOutClicked(object sender, EventArgs e)
	{
		// await MainThread.InvokeOnMainThreadAsync(async () =>
		// {
		//     await Shell.Current.GoToAsync("//signout", true);
		// });
		System.Diagnostics.Debug.WriteLine("[SafeArea] Navigation to SignOut page using shell");
		await Shell.Current.GoToAsync("//signout", false);

	}
}