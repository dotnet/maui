using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12344, "[Bug] FontImageSource does not work on UWP", PlatformAffected.UWP)]
	public partial class Issue12344 : TestContentPage
	{
		public Issue12344()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}