using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32447, "[iOS] App crash when scrolling quickly through a TableView that has Pickers in the cells.", PlatformAffected.iOS)]
	public partial class Bugzilla32447 : TestContentPage
	{
		public Bugzilla32447 ()
		{
			#if APP

			InitializeComponent ();

			#endif
		}

		protected override void Init ()
		{
			
		}
	}
}

