using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2981, "Long Press on ListView causes crash")]
	public class Issue2981 : TestContentPage
	{
	
		protected override void Init ()
		{
			var listView = new ListView ();

			listView.ItemsSource = new [] { "Cell1", "Cell2" };
			Content = listView;
		}

#if UITEST && !__WINDOWS__ 

		// This test won't work on Windows right now because we can only test desktop, so touch events
		// (like LongPress) don't really work. The test should work manually on a touch screen, though.

		[Test]
		public void Issue2981Test ()
		{
			RunningApp.Screenshot ("I am at Issue 1");
			RunningApp.TouchAndHold (q => q.Marked ("Cell1"));
			RunningApp.Screenshot ("Long Press first cell");
			RunningApp.TouchAndHold (q => q.Marked ("Cell2"));
			RunningApp.Screenshot ("Long Press second cell");
		}
#endif
	}
}
