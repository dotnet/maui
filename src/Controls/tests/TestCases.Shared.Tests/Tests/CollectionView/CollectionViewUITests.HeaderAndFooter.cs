using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
#if IOS
    public class CollectionViewHeaderAndFooterTests : CollectionViewUITests
    {

        protected override bool ResetAfterEachTest => true;

        public CollectionViewHeaderAndFooterTests(TestDevice device)
            : base(device)
        {
        }

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Header not rendering issue: https://github.com/dotnet/maui/issues/27177
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
#endif

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
    }
#endif
}
