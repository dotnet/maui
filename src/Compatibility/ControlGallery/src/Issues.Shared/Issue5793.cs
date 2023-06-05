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
	[Issue(IssueTracker.Github, 5793, "[CollectionView/ListView] Not listening for Reset command",
		PlatformAffected.iOS | PlatformAffected.Android)]
	class Issue5793 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
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
