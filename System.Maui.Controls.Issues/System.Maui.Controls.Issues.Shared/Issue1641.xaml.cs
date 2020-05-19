using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{	
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1641, "Static global::Android.Content.Res.Resources inside TableView using XAML doesn't work", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1641 : ContentPage
	{
		public Issue1641 ()
		{
			InitializeComponent ();
		}
	}
#endif
}
