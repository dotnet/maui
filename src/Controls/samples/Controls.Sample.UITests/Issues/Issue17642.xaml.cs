using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17642, "Button Click Ripple Effect Not Working In Android", PlatformAffected.Android)]
	public partial class Issue17642 : ContentPage
	{
		public Issue17642()
		{
			InitializeComponent();
		}
	}
}