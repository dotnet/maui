using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16094, "Shadows don't respect control shape", PlatformAffected.UWP)]
	public partial class Issue16094 : ContentPage
	{
		public Issue16094()
		{
			InitializeComponent();
		}
	}
}