using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18946, "Shell Toolbar items not displayed", PlatformAffected.All)]
	public partial class Issue18946 : Shell
	{
		public Issue18946()
		{
			InitializeComponent();
		}
	}

}