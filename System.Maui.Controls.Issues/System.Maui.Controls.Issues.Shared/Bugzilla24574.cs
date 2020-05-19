using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 24574, "Tap Double Tap")]
	public class Issue24574 : TestContentPage 
	{
		protected override void Init ()
		{
			var label = new Label {
				AutomationId = "TapLabel",
				Text = "123",
				FontSize = 50
			};

			var rec = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
			rec.Tapped += (s, e) => { label.Text = "Single"; };
			label.GestureRecognizers.Add (rec);

			rec = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
			rec.Tapped += (s, e) => { label.Text = "Double"; };
			label.GestureRecognizers.Add (rec);

			Content = label;
		}

#if UITEST
		[Test]
		public void TapThenDoubleTap ()
		{
			RunningApp.Screenshot ("I am at Issue 24574");

			RunningApp.WaitForElement (q => q.Marked ("TapLabel"));
			
			RunningApp.Tap (q => q.Marked ("TapLabel"));
			RunningApp.WaitForElement (q => q.Marked ("Single"));
			
			RunningApp.DoubleTap (q => q.Marked ("TapLabel"));
			RunningApp.WaitForElement (q => q.Marked ("Double"));
		}
#endif
	}
}
