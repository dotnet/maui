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
	[Issue(IssueTracker.None, 8675310, "CollectionView Header/Footer Strings", PlatformAffected.All)]
	public class CollectionViewHeaderFooterString : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.HeaderFooterGalleries.HeaderFooterString());
#endif
		}

#if UITEST
		[Test]
		public void CollectionViewHeaderAndFooterUsingStrings()
		{
			RunningApp.WaitForElement("Just a string as a header");
			RunningApp.WaitForElement("This footer is also a string");
		}
#endif
	}

}
