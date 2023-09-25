using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 11111111, "CollectionView Scroll To Grouped Item",
		PlatformAffected.All)]
	public class ScrollToGroup : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.ScrollToGalleries.ScrollToGroup());
#endif
		}

#if UITEST && __IOS__ // Grouping for Android hasn't been merged yet
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void CanScrollToGroupAndItemIndex()
		{
			RunningApp.WaitForElement("GroupIndexEntry");
			RunningApp.Tap("GroupIndexEntry");
			RunningApp.ClearText();
			RunningApp.EnterText("5");

			RunningApp.Tap("ItemIndexEntry");
			RunningApp.ClearText();
			RunningApp.EnterText("1");

			RunningApp.Tap("GoButton");

			// Should scroll enough to display this item
			RunningApp.WaitForElement("Squirrel Girl");
		}

		[Test]
		public void InvalidScrollToIndexShouldNotCrash()
		{
			RunningApp.WaitForElement("GroupIndexEntry");
			RunningApp.Tap("GroupIndexEntry");
			RunningApp.ClearText();
			RunningApp.EnterText("55");

			RunningApp.Tap("ItemIndexEntry");
			RunningApp.ClearText();
			RunningApp.EnterText("1");

			RunningApp.Tap("GoButton");

			// Should scroll enough to display this item
			RunningApp.WaitForElement("Avengers");
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void CanScrollToGroupAndItem()
		{
			RunningApp.WaitForElement("GroupNameEntry");
			RunningApp.Tap("GroupNameEntry");
			RunningApp.ClearText();
			RunningApp.EnterText("Heroes for Hire");

			RunningApp.Tap("ItemNameEntry");
			RunningApp.ClearText();
			RunningApp.EnterText("Misty Knight");

			RunningApp.Tap("GoItemButton");

			// Should scroll enough to display this item
			RunningApp.WaitForElement("Luke Cage");
		}
#endif
	}
}
