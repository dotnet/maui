namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 39636, "Cannot use XamlC with OnPlatform in resources, it throws System.InvalidCastException", PlatformAffected.All)]
	public partial class Bugzilla39636 : TestContentPage
	{
		public Bugzilla39636()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}
	}
}
