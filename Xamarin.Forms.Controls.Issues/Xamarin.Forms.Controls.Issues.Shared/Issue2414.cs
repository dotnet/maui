using System;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest.iOS;
using Xamarin.Forms.Core.UITests;
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
			RunningApp.ActivateContextMenu("Swipe ME");
			RunningApp.WaitForElement (c => c.Marked ("Text0"));
			RunningApp.Screenshot ("Didn't crash");
			RunningApp.Tap(c => c.Marked("Text0"));
		}

		[Test]
		public void TestShowContextMenuItemsInTheRightOrder ()
		{
			RunningApp.ActivateContextMenu("Swipe ME");
			RunningApp.WaitForElement (c => c.Marked ("Text0"));
			RunningApp.Screenshot ("Are the menuitems in the right order?");
			RunningApp.Tap(c => c.Marked("Text0"));
		}
#endif

	}
}


