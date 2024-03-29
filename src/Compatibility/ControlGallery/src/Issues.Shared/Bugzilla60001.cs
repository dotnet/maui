﻿using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60001, "[UWP] Inconsistency with DatePicker ", PlatformAffected.UWP)]
	public class Bugzilla60001 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			StackLayout layout = new StackLayout() { Orientation = StackOrientation.Vertical };
			DatePicker picker = new DatePicker();
			picker.Date = new DateTime(2017, 10, 7, 0, 0, 0, DateTimeKind.Utc);
			Label label = new Label() { Text = "On Droid this will show as 10/7/2017, on UWP it will show as 10/06/2017.  Local TimeZone for this test was EDT.", LineBreakMode = LineBreakMode.WordWrap };
			layout.Children.Add(picker);
			layout.Children.Add(label);
			// Initialize ui here instead of ctor
			Content = layout;
		}
	}
}