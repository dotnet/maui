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
	[Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 8888888, "CollectionView ItemsUpdatingScrollMode", PlatformAffected.All)]
	public class CollectionViewItemsUpdatingScrollMode : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.ScrollModeGalleries.ScrollModeTestGallery());
#endif
		}

#if UITEST
		[MovedToAppium]
		[Test]
		public void KeepItemsInView()
		{
			RunningApp.WaitForElement("ScrollToMiddle");
			RunningApp.Tap("ScrollToMiddle");	
			RunningApp.WaitForElement("Vegetables.jpg, 10");
			for (int n = 0; n < 25; n++)
			{
				RunningApp.Tap("AddItemAbove");
			}
			RunningApp.WaitForElement("Vegetables.jpg, 10");
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void KeepScrollOffset()
		{
			RunningApp.WaitForElement("SelectScrollMode");
			RunningApp.Tap("SelectScrollMode");
			RunningApp.Tap("KeepScrollOffset");

			RunningApp.WaitForElement("ScrollToMiddle");
			RunningApp.Tap("ScrollToMiddle");	
			RunningApp.WaitForElement("Vegetables.jpg, 10");
			RunningApp.Tap("AddItemAbove");	
			RunningApp.WaitForElement("photo.jpg, 9");
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void KeepLastItemInView()
		{
			RunningApp.WaitForElement("SelectScrollMode");
			RunningApp.Tap("SelectScrollMode");
			RunningApp.Tap("KeepLastItemInView");

			RunningApp.WaitForElement("ScrollToMiddle");
			RunningApp.Tap("ScrollToMiddle");	
			RunningApp.WaitForElement("Vegetables.jpg, 10");
			RunningApp.Tap("AddItemToEnd");	
			RunningApp.WaitForElement("Added item");
		}
#endif
	}
}
