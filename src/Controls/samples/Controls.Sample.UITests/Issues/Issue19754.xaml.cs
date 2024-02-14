using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19754, "Investigate adding coordinate clicking to the Tap feature for catalyst", PlatformAffected.macOS)]
	public partial class Issue19754 : ContentPage
	{
		public Issue19754()
		{
			InitializeComponent();
		}

		void OnTapped(object sender, TappedEventArgs e)
		{
			TestLabel.Text = "Tapped";
		}
	}
}