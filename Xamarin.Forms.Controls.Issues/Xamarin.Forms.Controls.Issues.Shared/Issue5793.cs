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
	[Issue(IssueTracker.Github, 5793, "[CollectionView/ListView] Not listening for Reset command",
		PlatformAffected.iOS | PlatformAffected.Android)]
	class Issue5793 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			PushAsync(new GalleryPages.CollectionViewGalleries.ObservableCollectionResetGallery());
#endif
		}

#if UITEST
		[Test]
		public void Reset()
		{
			RunningApp.WaitForElement("Reset");

			// Verify the item is there
			RunningApp.WaitForElement("cover1.jpg, 0");

			// Clear the collection
			RunningApp.Tap("Reset");	

			// Verify the item is gone
			RunningApp.WaitForNoElement("cover1.jpg, 0");
		}
#endif
	}
}
