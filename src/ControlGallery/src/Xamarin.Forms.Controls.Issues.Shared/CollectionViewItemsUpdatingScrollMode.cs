using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 8888888, "CollectionView ItemsUpdatingScrollMode", PlatformAffected.All)]
	public class CollectionViewItemsUpdatingScrollMode : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			PushAsync(new GalleryPages.CollectionViewGalleries.ScrollModeGalleries.ScrollModeTestGallery());
#endif
		}

#if UITEST
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
