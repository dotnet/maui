using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22755, "[Android] TabbedPage in FlyoutPage in navigationPage does not fit all screen", PlatformAffected.Android)]
	public partial class Issue22755 : FlyoutPage
	{
		public Issue22755()
		{
			InitializeComponent();
		}
	}
}