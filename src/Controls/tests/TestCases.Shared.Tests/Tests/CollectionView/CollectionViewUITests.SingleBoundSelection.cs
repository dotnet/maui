using NUnit.Framework;
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

		/*
		// SelectionShouldUpdateBinding (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewBoundSingleSelection.cs)
		[Test]
		[Ignore("Click does not find CollectionView elements")]
		[Description("Single Selection Binding")]
		public void SelectionShouldUpdateBinding()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac, TestDevice.Windows },
				"Click does not find CollectionView elements");

			// Navigate to the selection galleries
			VisitInitialGallery("Selection");
			
			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("SingleSelection,Bound");
			
			// 1. Initially Item 2 should be selected (from the view model)
			App.WaitForNoElement("Selected: Item: 2");

			// 2. Tapping Item 3 should select it and updating the binding
			App.Click("Item 1");
			App.WaitForNoElement("Selected: Item: 1");
		}
		*/
	}
}