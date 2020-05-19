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
	[Issue(IssueTracker.None, 47803, "CollectionView: Multi Selection Binding", PlatformAffected.All)]
	public class CollectionViewBoundMultiSelection : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			PushAsync(new GalleryPages.CollectionViewGalleries.SelectionGalleries.MultipleBoundSelection());
#endif
		}

#if UITEST
		[Test]
		public void ItemsFromViewModelShouldBeSelected()
		{
			// Initially Items 1 and 2 should be selected (from the view model)
			RunningApp.WaitForElement("Selected: Item 1, Item 2");

			// Tapping Item 3 should select it and updating the binding
			RunningApp.Tap("Item 3");	
			RunningApp.WaitForElement("Selected: Item 1, Item 2, Item 3");

			// Test clearing the selection from the view model and updating it
			RunningApp.Tap("ClearAndAdd");	
			RunningApp.WaitForElement("Selected: Item 1, Item 2");

			// Test removing an item from the selection
			RunningApp.Tap("Item 2");
			RunningApp.WaitForElement("Selected: Item 1");

			// Test setting a new selection list in the view mdoel 
			RunningApp.Tap("Reset");	
			RunningApp.WaitForElement("Selected: Item 1, Item 2");

			RunningApp.Tap("Item 0");
			
			// Test setting the selection directly with CollectionView.SelectedItems 
			RunningApp.Tap("DirectUpdate");	
			RunningApp.WaitForElement("Selected: Item 0, Item 3");
		}
#endif
	}
}