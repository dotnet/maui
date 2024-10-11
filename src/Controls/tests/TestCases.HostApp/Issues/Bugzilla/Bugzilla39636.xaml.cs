using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39636, "Cannot use XamlC with OnPlatform in resources, it throws System.InvalidCastException", PlatformAffected.All)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
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
