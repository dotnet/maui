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
	[Issue(IssueTracker.None, 8675311, "CollectionView Header/Footer Template", PlatformAffected.All)]
	public class CollectionViewHeaderFooterTemplate : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.HeaderFooterGalleries.HeaderFooterTemplate());
#endif
		}

#if UITEST
		[Test]
		public void CollectionViewHeaderAndFooterUsingTemplates()
		{
			RunningApp.WaitForElement("This Is A Header");
			RunningApp.WaitForElement("This Is A Footer");
		}
#endif

	}
}
