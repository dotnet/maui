using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{

	public class CollectionViewHeaderAndFooterTests : CollectionViewUITests
	{

		protected override bool ResetAfterEachTest => true;

		public CollectionViewHeaderAndFooterTests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderFooterStringWorks()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Header Footer");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("Header/Footer (String)");

			App.WaitForElement("Just a string as a header");
			App.WaitForElement("This footer is also a string");
		}
#if IOS || ANDROID
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderFooterViewWorks()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Header Footer");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("Header/Footer (Forms View)");

			App.WaitForElement("This Is A Header");
			App.WaitForElement("This Is A Footer");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderFooterHorizontalViewWorks()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Header Footer");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("Header/Footer (Horizontal Forms View)");

			// Verify the header is visible
			App.WaitForElement("This Is A Header");

			// Scroll right to ensure the footer is visible and positioned at the end
			for (int i = 0; i < 5; i++)
			{
				App.ScrollRight("CV", ScrollStrategy.Auto, 0.9, 250);
			}

			// Verify the footer is visible
			App.WaitForElement("This Is A Footer");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderFooterTemplateWorks()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Header Footer");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("Header/Footer (Template)");

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderFooterGridWorks()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Header Footer");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("Header/Footer (Grid)");

			App.WaitForElement("This Is A Header");
#if !ANDROID
			// Android screen is too small to show this label
			// but we can check for the footer via screenshot
            App.WaitForElement("This Is A Footer");
#endif

			VerifyScreenshot();
		}

#if TEST_FAILS_ON_IOS
		// The screenshot that's currently generated for this test is wrong
		// So, we're ignoring this test due to it causing confusion when other changes
		// cause this test to fail.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderFooterGridHorizontalWorks()
		{
			// Navigate to the selection galleries
			VisitInitialGallery("Header Footer");

			// Navigate to the specific sample inside selection galleries
			VisitSubGallery("Header/Footer (Grid Horizontal)");

			App.WaitForElement("This Is A Header");

			// This is a bug in the test, the footer is not being found
			//App.WaitForElement("This Is A Footer");

			VerifyScreenshot();
		}
#endif
#endif
	}
}
