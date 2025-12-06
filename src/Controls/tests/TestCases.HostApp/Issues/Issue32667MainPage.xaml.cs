namespace Maui.Controls.Sample.Issues;

public partial class Issue32667MainPage : ContentPage
{
	public Issue32667MainPage()
	{
		InitializeComponent();
	}
	
	private async void OnNavigateToSubPage(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("subpage");
	}
}
