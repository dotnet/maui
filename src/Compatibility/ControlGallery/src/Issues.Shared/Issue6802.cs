using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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