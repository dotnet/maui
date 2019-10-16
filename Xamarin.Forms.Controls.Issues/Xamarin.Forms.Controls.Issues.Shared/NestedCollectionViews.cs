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
	[Issue(IssueTracker.Github, 6620, "[iOS] Crash when creating a CollectionView inside a CollectionView",
		PlatformAffected.iOS | PlatformAffected.UWP)]
	public class NestedCollectionViews : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.NestedGalleries.NestedCollectionViewGallery());
#endif
		}

#if UITEST
		[Test]
		public void NestedCollectionViewsShouldNotCrash()
		{
			// If this page loaded and didn't crash, we're good.
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
