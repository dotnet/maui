namespace Maui.Controls.Sample.Issues
{
	public partial class MainTabPage : ContentPage
	{
		public MainTabPage()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
		await Task.Delay(500);
		base.OnAppearing();
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