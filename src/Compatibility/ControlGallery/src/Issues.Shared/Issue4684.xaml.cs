using System;
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
	[Issue(IssueTracker.Github, 4684, "[Android] don't clear shell content because native page isn't visible",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public sealed partial class Issue4684 : TestShell
	{
		public Issue4684()
		{
#if APP
			this.InitializeComponent();
#endif
		}

		protected override void Init()
		{
		}

#if UITEST
		[Test]
		public void NavigatingBackAndForthDoesNotCrash()
		{
			TapInFlyout("Connect");
			RunningApp.Tap("Control");

			TapInFlyout("Home");
			TapInFlyout("Connect");

			RunningApp.Tap("Connect");
			RunningApp.Tap("Control");

			RunningApp.WaitForElement("Success");
		}

#endif
	}
}
