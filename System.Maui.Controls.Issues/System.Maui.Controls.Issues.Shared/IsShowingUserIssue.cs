using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "IsShowingUser renderes as pin instead of dot")]
	public class IsShowingUserIssue : TestContentPage
	{
		protected override void Init ()
		{
			var map = new Map (MapSpan.FromCenterAndRadius (new Position(37.79, -122.4), Distance.FromMiles(2))) {
				AutomationId = "FormsMap",
				IsShowingUser = true
			};

			Content = map;
		}

#if UITEST
		public void IsShowingUserIssueTest ()
		{
			RunningApp.Screenshot ("I should see a map");
			Assert.Inconclusive ("Verify that user location is visible and a dot");
		}
#endif
	}
}
