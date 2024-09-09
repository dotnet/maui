using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21202, "FontImageSource incorrectly sized", PlatformAffected.UWP)]
	public partial class Issue21202 : ContentPage
	{
		public Issue21202()
		{
			InitializeComponent();
		}
	}
}