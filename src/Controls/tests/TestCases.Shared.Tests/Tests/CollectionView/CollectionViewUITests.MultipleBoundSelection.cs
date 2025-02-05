#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Multiselection not working in Catalyst and iOS, Issue: https://github.com/dotnet/maui/issues/26942 & https://github.com/dotnet/maui/issues/26943
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class CollectionViewBoundMultiSelectionUITests : CollectionViewUITests
	{
		public CollectionViewBoundMultiSelectionUITests(TestDevice device)
			: base(device)
		{
		}

		// ItemsFromViewModelShouldBeSelected (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewBoundMultiSelection.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ItemsFromViewModelShouldBeSelected()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Selection");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("MultipleSelection,Bound");

			// 1. Initially Items 1 and 2 should be selected (from the view model)
			App.WaitForElement("Selected: Item 1, Item 2");

			// 2. Tapping Item 3 should select it and updating the binding
			App.Tap("Item 3");

			App.WaitForElement("Selected: Item 1, Item 2, Item 3");

			// 3. Test clearing the selection from the view model and updating it
			App.Tap("ClearAndAdd");
			App.WaitForElement("Selected: Item 1, Item 2");

			// 4. Test removing an item from the selection
			App.Tap("Item 2");
			App.WaitForElement("Selected: Item 1");

			// 5. Test setting a new selection list in the view mdoel 
			App.Tap("Reset");
			App.WaitForElement("Selected: Item 1, Item 2");

			App.Tap("Item 0");

			// 6. Test setting the selection directly with CollectionView.SelectedItems
			App.WaitForElement("DirectUpdate");
			App.Tap("DirectUpdate");
			App.WaitForElement("Selected: Item 0, Item 3");
		}
	}
}
#endif