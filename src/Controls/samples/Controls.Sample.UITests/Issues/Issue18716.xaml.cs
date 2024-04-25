using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18716, "Touch events are not working on WebView when a PDF is displayed", PlatformAffected.iOS)]
	public partial class Issue18716 : ContentPage
	{
		public Issue18716()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			WaitForStubControl.Navigating += OnWebViewNavigating;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			WaitForStubControl.Navigating -= OnWebViewNavigating;
		}
		
		void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
		{
			LoadedControl.Text = "Ready";
		}
	}
}