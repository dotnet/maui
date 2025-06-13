using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class CollectionViewSingleBoundSelectionUITests : CollectionViewUITests
	{
		public CollectionViewSingleBoundSelectionUITests(TestDevice device)
			: base(device)
		{
		}

		[Fact]
		[Category(UITestCategories.CollectionView)]
		[Description("Single Selection Binding")]
		public void SelectionShouldUpdateBinding()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Selection");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("SingleSelection,Bound");

			// 1. Initially Item 2 should be selected (from the view model)
			App.WaitForElement("Selected: Item: 2");

			// 2. Tapping Item 3 should select it and updating the binding
			App.Click("Item 1");
			App.WaitForElement("Selected: Item: 1");
		}

	}
}