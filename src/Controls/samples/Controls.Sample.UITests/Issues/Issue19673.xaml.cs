using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19673, "ToolbarItem icon/image doesn't update on page navigation on Android", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	public partial class Issue19673 : TabbedPage
	{
		public Issue19673()
		{
			InitializeComponent();
		}
	}
}