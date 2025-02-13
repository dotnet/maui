#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17022 : _IssuesUITest
	{
		public Issue17022(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "UINavigationBar is Translucent";

		// TODO: Add shell navigation bar tests when we can call shell in UITest
		[Test, Retry(2)]
		[Category(UITestCategories.Navigation)]
		[TestCase("NewNavigationPageButton", false), Retry(2), Retry(2)]
        [TestCase("NewNavigationPageTransparentButton", false), Retry(2), Retry(2)]
        [TestCase("NewNavigationPageTranslucentButton", false), Retry(2), Retry(2)]
        [TestCase("NewNavigationPageTransparentTranslucentButton", false), Retry(2), Retry(2)]
        [TestCase("NewNavigationPageGridButton", false), Retry(2), Retry(2)]
        [TestCase("NewNavigationPageGridTransparentButton", false), Retry(2), Retry(2)]
        [TestCase("NewNavigationPageGridTranslucentButton", false, true), Retry(2), Retry(2)] // this test thinks the boxview is at the top of the screen, but it's not. Test this case manually for now.
        [TestCase("NewNavigationPageGridTransparentTranslucentButton", true), Retry(2), Retry(2)]
        [TestCase("NewFlyoutPageButton", false), Retry(2), Retry(2)]
        [TestCase("NewFlyoutPageTransparentButton", false), Retry(2), Retry(2)]
        [TestCase("NewFlyoutPageTranslucentButton", false), Retry(2), Retry(2)]
        [TestCase("NewFlyoutPageTransparentTranslucentButton", false), Retry(2), Retry(2)]
        [TestCase("NewFlyoutPageGridButton", false), Retry(2), Retry(2)]
        [TestCase("NewFlyoutPageGridTransparentButton", false), Retry(2), Retry(2)]
        [TestCase("NewFlyoutPageGridTranslucentButton", false, true), Retry(2), Retry(2)] // this test thinks the boxview is at the top of the screen, but it's not. Test this case manually for now.
        [TestCase("NewFlyoutPageGridTransparentTranslucentButton", true), Retry(2), Retry(2)]
        [TestCase("SemiTransparentNavigationPageBackgroundColor", true, true), Retry(2), Retry(2)]
        [TestCase("SemiTransparentNavigationPageBrush", true, true), Retry(2), Retry(2)]
        [TestCase("SemiTransparentFlyoutPageBackgroundColor", true, true), Retry(2), Retry(2)]
        [TestCase("SemiTransparentFlyoutPageBrush", true, true), Retry(2), Retry(2)]

		public void Issue17022Test(string testButtonID, bool isTopOfScreen, bool requiresScreenshot = false)
		{
            App.WaitForElement(testButtonID).Click();
            var boxView = App.WaitForElement("TopBoxView");
            ClassicAssert.NotNull(boxView);
			var rect = boxView.GetRect();

            try { 
                if (requiresScreenshot)
                {
                    VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + testButtonID);
                }
                else
                {
                    if (isTopOfScreen)
                    {
                        ClassicAssert.AreEqual(rect.Y, 0);
                    }
                    else
                    {
						ClassicAssert.AreNotEqual(rect.Y, 0);
                    }
                }
            }
            catch 
            { 
                Assert.Fail("Failed with exception");
            }
            finally
            {
                App.WaitForElement("PopPageButton").Click();
            }
		}
	}
}
#endif