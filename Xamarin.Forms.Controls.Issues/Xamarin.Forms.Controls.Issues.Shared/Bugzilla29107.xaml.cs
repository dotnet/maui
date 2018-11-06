using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 29107, "Xamarin.Android ScrollView text overlaps", PlatformAffected.Android)]
	public partial class Bugzilla29107 : TestContentPage
	{
		public Bugzilla29107 ()
		{
#if APP
			InitializeComponent ();
#endif
		}

		protected override void Init ()
		{
			
		}

	}
}
