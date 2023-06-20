using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8526, "[Bug] DisplayPromptAsync hangs app, doesn't display when called in page load",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.DisplayPrompt)]
#endif
	public class Issue8526 : TestContentPage
	{
		const string Success = "Success";

		protected override async void Init()
		{
			await DisplayPromptAsync(Success, "This prompt should display when the page loads.");
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void DisplayPromptShouldWorkInPageLoad()
		{
			RunningApp.WaitForElement(Success);
			RunningApp.Tap("Cancel");
		}
#endif
	}
}
