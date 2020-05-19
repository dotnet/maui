using System;
using System.Collections.Generic;

using System.Maui.CustomAttributes;

using System.Maui;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 27318, "Labels overlapping", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	public partial class Bugzilla27318 : ContentPage
	{
		public Bugzilla27318 ()
		{
			InitializeComponent ();
			listView.ItemsSource = new [] { "Foo", "Bar", "Baz" };
		}
	}
#endif
}
