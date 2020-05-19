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
	[Issue(IssueTracker.None, 4539134, "CollectionView: Single Selection Binding", PlatformAffected.All)]
	public class CollectionViewBoundSingleSelection : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			PushAsync(new GalleryPages.CollectionViewGalleries.SelectionGalleries.SingleBoundSelection());
#endif
		}

#if UITEST
		[Test]
		public void SelectionShouldUpdateBinding()
		{
			// Initially Item 2 should be selected (from the view model)
			RunningApp.WaitForElement("Selected: Item: 2");

			// Tapping Item 3 should select it and updating the binding
			RunningApp.Tap("Item 3");	
			RunningApp.WaitForElement("Selected: Item: 3");
		}
#endif
	}
}
