using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6802, "[Bug] Shell SearchHandler - Cancel is visible by default?", PlatformAffected.iOS)]
	public class Issue6802 : TestShell
	{
		protected override void Init()
		{
			var cp = CreateContentPage();

			Shell.SetSearchHandler(cp, new SearchHandler());
		}
	}
}