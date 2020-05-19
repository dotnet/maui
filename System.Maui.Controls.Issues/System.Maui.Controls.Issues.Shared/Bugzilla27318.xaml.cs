using System;
using System.Collections.Generic;

using Xamarin.Forms.CustomAttributes;

using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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
