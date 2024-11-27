﻿using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 33578, "TableView EntryCell shows DefaultKeyboard, but after scrolling down and back a NumericKeyboard (")]
	public class Bugzilla33578 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Content = new TableView
			{
				Root = new TableRoot {
					new TableSection {
						new EntryCell {
							Placeholder = "Enter text here 1",
							AutomationId = "entryNormal"
						},
						new EntryCell {
							Placeholder = "Enter text here 2"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here"
						},
						new EntryCell {
							Placeholder = "Enter text here",
							AutomationId = "entryPreviousNumeric"
						},
						new EntryCell {
							Keyboard = Keyboard.Numeric,
							Placeholder = "0",
							AutomationId = "entryNumeric"
						}
					}
				}
			};
		}

#if UITEST && __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TableViewEntryCellShowsDefaultKeyboardThenNumericKeyboardAfterScrolling()
		{
			RunningApp.ScrollDown();
			RunningApp.ScrollDown();
			RunningApp.Tap(x => x.Marked("0"));
			var e = RunningApp.Query(c => c.Marked("0").Parent("UITextField").Index(0).Invoke("keyboardType"))[0];
			//8 DecimalPad
			Assert.AreEqual(8, e);
			RunningApp.DismissKeyboard();
			RunningApp.Tap(x => x.Marked("Enter text here").Index(0).Parent());
			RunningApp.ScrollUp();
			RunningApp.Tap(x => x.Marked("Enter text here 1"));
			RunningApp.Tap(x => x.Marked("Enter text here 2").Index(0).Parent());
			var e1 = RunningApp.Query(c => c.Marked("Enter text here 2").Parent("UITextField").Index(0).Invoke("keyboardType"))[0];
			Assert.AreEqual(0, e1);
		}
#endif
	}
}
