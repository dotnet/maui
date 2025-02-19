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
	[Issue(IssueTracker.Github, 6804, "[Bug] Shell SearchHandler - Search text getting lost on execute search", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue6804 : TestShell
	{
		protected override void Init()
		{
			var cp = CreateContentPage();
			cp.Content = new Label
			{
				Text = "Enter a search query and execute. The query should remain and not be cleared."
			};
			Shell.SetSearchHandler(cp, new SearchHandler());

		}
	}
}