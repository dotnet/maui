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
	[Issue(IssueTracker.None, 4539134, "CollectionView: Single Selection Binding", PlatformAffected.All)]
	public class CollectionViewBoundSingleSelection : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.CollectionViewGalleries.SelectionGalleries.SingleBoundSelection());
#endif
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void SelectionShouldUpdateBinding()
		{
			// Initially Item 2 should be selected (from the view model)
			RunningApp.WaitForElement("Selected: Item: 2");

			// Tapping Item 3 should select it and updating the binding
			RunningApp.Tap("Item 1");	
			RunningApp.WaitForElement("Selected: Item: 1");
		}
#endif
	}
}
