using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19309, "Nightly build 9725 broke boxviews do not get their color set anymore", PlatformAffected.macOS | PlatformAffected.iOS)]
	public partial class Issue19309 : ContentPage
	{
		public Issue19309()
		{
			InitializeComponent();
		}
	}
}