using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27318, "Labels overlapping", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	public partial class Bugzilla27318 : ContentPage
	{
		public Bugzilla27318()
		{
			InitializeComponent();
			listView.ItemsSource = new[] { "Foo", "Bar", "Baz" };
		}
	}
#endif
}
