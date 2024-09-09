using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18242, "Button ImageSource not Scaling as expected", PlatformAffected.UWP)]
	public partial class Issue18242 : ContentPage
	{
		public Issue18242()
		{
			InitializeComponent();
		}
	}
}