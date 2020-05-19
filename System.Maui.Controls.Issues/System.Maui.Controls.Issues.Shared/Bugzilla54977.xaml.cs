using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 54977, "Toolbaritems do not appear", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	public partial class Bugzilla54977 : ContentPage
	{
		string _prefix;

		public Bugzilla54977()
		{
			InitializeComponent();
		}
	}
#endif
}
