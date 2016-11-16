using System;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2414, "NullReferenceException when swiping over Context Actions", PlatformAffected.WinPhone)]
	public class Issue2414 : TestContentPage
	{
		protected override void Init ()
		{
			var tableView = new TableView
			{
				Intent = TableIntent.Settings,
				Root = new TableRoot("TableView Title")
				{
					new TableSection("Table Section 2")
					{
						new TextCell
						{
							Text = "Swipe ME",
							Detail = "And I will crash!",
							ContextActions = { 
								new MenuItem
								{
									Text = "Text0"
								},new MenuItem
								{
									Text = "Text1"
								},
								new MenuItem
								{
									Text = "Text2"
								},
								new MenuItem
								{
									Text = "Text3"
								},
								new MenuItem
								{
									Text = "Text4",
									IsDestructive = true,
								}}
						},
					}
				}
			};
			Content = tableView;
		}

#if UITEST
		[Test]
		public void TestDoesntCrashShowingContextMenu ()
		{
			RunningApp.WaitForElement(c => c.Marked("Swipe ME"));

			var screenBounds = RunningApp.Query (q => q.Raw ("* index:0"))[0].Rect;

			var cell = RunningApp.Query(c => c.Marked("Swipe ME")) [0];
#if __IOS__
			RunningApp.DragCoordinates (screenBounds.Width - 10, cell.Rect.CenterY, 0, cell.Rect.CenterY);
			//TODO: fix this when context menu bug is fixed
			RunningApp.WaitForElement (c => c.Marked ("Text4"));
#else
			RunningApp.TouchAndHoldCoordinates (cell.Rect.CenterX, cell.Rect.CenterY);
			RunningApp.WaitForElement (c => c.Marked ("Text0"));
#endif
			RunningApp.Screenshot ("Didn't crash");
			RunningApp.TapCoordinates (screenBounds.CenterX, screenBounds.CenterY);

#if __ANDROID__
			RunningApp.Tap(c => c.Marked("Text0"));
#endif

		}

		[Test]
		public void TestShowContextMenuItemsInTheRightOrder ()
		{
			RunningApp.WaitForElement(c => c.Marked("Swipe ME"));

			var screenBounds = RunningApp.Query (q => q.Raw ("* index:0"))[0].Rect;

			var cell = RunningApp.Query (c => c.Marked ("Swipe ME")) [0];
#if __IOS__
			RunningApp.DragCoordinates (screenBounds.Width -10, cell.Rect.CenterY, 0, cell.Rect.CenterY);
#else
			RunningApp.TouchAndHoldCoordinates (cell.Rect.CenterX, cell.Rect.CenterY);
#endif
			RunningApp.WaitForElement (c => c.Marked ("Text0"));
			RunningApp.Screenshot ("Are the menuitems in the right order?");

#if __ANDROID__
			RunningApp.Tap(c => c.Marked("Text0"));
#endif

		}
#endif

	}
}


