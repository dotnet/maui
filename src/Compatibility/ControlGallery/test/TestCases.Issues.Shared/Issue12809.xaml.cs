using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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