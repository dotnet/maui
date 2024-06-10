using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21631, "Injecting base tag in Webview2 works", PlatformAffected.UWP)]
	public partial class Issue21631 : ContentPage
	{
		public Issue21631()
		{
			InitializeComponent();
		}
	}
}