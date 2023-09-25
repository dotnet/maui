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
	[Issue(IssueTracker.None, 8675312, "CollectionView Header/Footer View", PlatformAffected.All)]
	public class CollectionViewHeaderFooterView : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.HeaderFooterGalleries.HeaderFooterView());
#endif
		}

#if UITEST
		[Test]
		public void CollectionViewHeaderAndFooterUsingViews()
		{
			RunningApp.WaitForElement("This Is A Header");
			RunningApp.WaitForElement("This Is A Footer");
		}
#endif
	}
}
