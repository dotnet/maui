using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest.iOS;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2414, "NullReferenceException when swiping over Context Actions", PlatformAffected.WinPhone)]
	public class Issue2414 : TestContentPage
	{
		protected override void Init()
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
		[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TestDoesntCrashShowingContextMenu()
		{
			RunningApp.ActivateContextMenu("Swipe ME");
			RunningApp.WaitForElement(c => c.Marked("Text0"));
			RunningApp.Screenshot("Didn't crash");
			RunningApp.Tap(c => c.Marked("Text0"));
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TestShowContextMenuItemsInTheRightOrder()
		{
			RunningApp.ActivateContextMenu("Swipe ME");
			RunningApp.WaitForElement(c => c.Marked("Text0"));
			RunningApp.Screenshot("Are the menuitems in the right order?");
			RunningApp.Tap(c => c.Marked("Text0"));
		}
#endif

	}
}


