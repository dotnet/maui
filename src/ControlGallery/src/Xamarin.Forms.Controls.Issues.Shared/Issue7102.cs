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
	[Issue(IssueTracker.Github, 7102, "[Bug] CollectionView Header cause delay to adding items.",
		PlatformAffected.Android)]
	public class Issue7102 : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.ObservableCodeCollectionViewGallery(grid: false));
#endif
		}

#if UITEST
		[Test]
		public void HeaderDoesNotBreakIndexes()
		{
			RunningApp.WaitForElement("entryInsert");
			RunningApp.Tap("entryInsert");
			RunningApp.ClearText();
			RunningApp.EnterText("1");
			RunningApp.Tap("Insert");

			// If the bug is still present, then there will be 
			// two "Item: 0" items instead of the newly inserted item
			// Or the header will have disappeared
			RunningApp.WaitForElement("Inserted");
			RunningApp.WaitForElement("This is the header");
		}
#endif
	}
}
