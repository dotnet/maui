namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18443, "SelectionLength Property Not Applied to Entry at Runtime", PlatformAffected.Android)]
	public partial class Issue18443 : ContentPage
	{
		public Issue18443()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(1000);
			MauiEntry.Focus();
		}


	}
}