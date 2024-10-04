namespace Maui.Controls.Sample.Issues
{
	public partial class MainTabPage : ContentPage
	{
		public MainTabPage()
		{
			InitializeComponent();
		}

		async void OpenNonTabbedPage(object sender, EventArgs args)
		{
			await Shell.Current.GoToAsync("nontabbedpage");
		}

		async void OpenInnerTabbedPage(object sender, EventArgs args)
		{
			await Shell.Current.GoToAsync("innertabbedpage");
		}
	}
}