using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20858, "FlyoutPage Android app crashing on orientation change", PlatformAffected.Android)]
	//[Issue(IssueTracker.Github, 18161, "Toggling FlyoutLayoutBehavior on Android causes the app to crash", PlatformAffected.Android)]
	public partial class Issue20858And18161 : FlyoutPage
	{
		public Issue20858And18161()
		{
			InitializeComponent();
			IsPresented = true;
		}
	}
}