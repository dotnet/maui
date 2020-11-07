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
	[Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9929, "[Bug] NSInternalInconsistencyException when trying to run XamarinTV on iOS",
		PlatformAffected.iOS)]
	public class Issue9929 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SpacingGalleries.SpacingGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)));
#endif
		}

#if UITEST
		[Test]
		public void InsanelyWideHorizontalSpacingShouldNotCrash()
		{
			RunningApp.WaitForElement("entryUpdate_Spacing");
			RunningApp.Tap("entryUpdate_Spacing");	
			RunningApp.ClearText();
			RunningApp.EnterText("0,500");
			RunningApp.Tap("btnUpdate_Spacing");	

			// If it hasn't crashed, we should still be able to find this
			RunningApp.WaitForElement("entryUpdate_Spacing");
		}
#endif
	}
}
