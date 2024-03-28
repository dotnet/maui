using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19099, "TapGestureRecognizer no longer works on Button", PlatformAffected.iOS)]
	public partial class Issue19099 : ContentPage
	{
		public Issue19099()
		{
			InitializeComponent();
		}

		void OnTapped(object sender, TappedEventArgs e)
		{
			TestLabel.Text = "Success";
		}
	}
}