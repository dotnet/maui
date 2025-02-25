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
        public void HeaderFooterTemplateWorks()
        {
            // Navigate to the selection galleries
            VisitInitialGallery("Header Footer");

            // Navigate to the specific sample inside selection galleries
            VisitSubGallery("Header/Footer (Template)");

            App.WaitForElement("This Is A Header");
            App.WaitForElement("This Is A Footer");

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
            App.WaitForElement("This Is A Footer");

            VerifyScreenshot();
        }

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST 
        [Test]
        [Category(UITestCategories.CollectionView)]
        public void HeaderFooterGridHorizontalWorks()
        {
            // Navigate to the selection galleries
            VisitInitialGallery("Header Footer");

            // Navigate to the specific sample inside selection galleries
            VisitSubGallery("Header/Footer (Grid Horizontal)");

            App.WaitForElement("This Is A Header");
            
 #if ANDROID
            App.ScrollRight("CollectionViewAutomation",ScrollStrategy.Gesture, 0.99,500);
            App.ScrollRight("CollectionViewAutomation",ScrollStrategy.Gesture, 0.99,500);
#endif 
            App.WaitForElement("This Is A Footer");

            VerifyScreenshot();
        }
#endif
	}
}