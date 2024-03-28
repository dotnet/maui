using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19380, "Switch control ignores OnColor and ThumbColor", PlatformAffected.iOS | PlatformAffected.Android)]
	public partial class Issue19380 : ContentPage
	{
		public Issue19380()
		{
			InitializeComponent();
		}
	}
}