using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20896, "Mac Idiom (Device family = 6) Button styling properties not working", PlatformAffected.macOS)]
	public partial class Issue20896 : ContentPage
	{
		public Issue20896()
		{
			InitializeComponent();
		}
	}
}