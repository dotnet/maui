using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{	
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1641, "Static Resources inside TableView using XAML doesn't work", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1641 : ContentPage
	{
		public Issue1641 ()
		{
			InitializeComponent ();
		}
	}
#endif
}
