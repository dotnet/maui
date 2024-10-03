namespace Maui.Controls.Sample.Issues;

public partial class NonTabbedPage : ContentPage
{
	public NonTabbedPage()
	{
		InitializeComponent();
	}

	async void OpenNonTabbedPage(object sender, EventArgs args)
	{
		await Shell.Current.GoToAsync("innertabbedpage");
	}
}
