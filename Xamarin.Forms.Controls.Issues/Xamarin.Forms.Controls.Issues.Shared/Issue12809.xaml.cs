using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12809, "[Bug] Entry text is invisible on iOS", PlatformAffected.iOS)]
	public partial class Issue12809 : TestContentPage
	{
		protected override void Init()
		{

		}

#if APP
		public Issue12809()
		{
			InitializeComponent();
		}
#endif
	}
}