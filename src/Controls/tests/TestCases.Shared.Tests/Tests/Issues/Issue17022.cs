#if IOS // This test case verifies that the UINavigationBar behaves as expected (translucent or transparent) exclusively on the iOS platform.
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
        [Test]
        [Category(UITestCategories.Navigation)]
        [TestCase("NewNavigationPageButton", false)]
        [TestCase("NewNavigationPageTransparentButton", false)]
        [TestCase("NewNavigationPageTranslucentButton", false)]
        [TestCase("NewNavigationPageTransparentTranslucentButton", false)]
        [TestCase("NewNavigationPageGridButton", false)]
        [TestCase("NewNavigationPageGridTransparentButton", true)] // if we set BarBackgroundColor to transparent, the boxview should be at the top.
        [TestCase("NewNavigationPageGridTranslucentButton", false, true)] // this test thinks the boxview is at the top of the screen, but it's not. Test this case manually for now.
        [TestCase("NewNavigationPageGridTransparentTranslucentButton", true)]
        [TestCase("NewFlyoutPageButton", false)]
        [TestCase("NewFlyoutPageTransparentButton", false)]
        [TestCase("NewFlyoutPageTranslucentButton", false)]
        [TestCase("NewFlyoutPageTransparentTranslucentButton", false)]
        [TestCase("NewFlyoutPageGridButton", false)]
        [TestCase("NewFlyoutPageGridTransparentButton", true)] // if we set BarBackgroundColor to transparent, the boxview should be at the top.
        [TestCase("NewFlyoutPageGridTranslucentButton", false, true)] // this test thinks the boxview is at the top of the screen, but it's not. Test this case manually for now.
        [TestCase("NewFlyoutPageGridTransparentTranslucentButton", true)]
        [TestCase("SemiTransparentNavigationPageBackgroundColor", true, true)]
        [TestCase("SemiTransparentNavigationPageBrush", true, true)]
        [TestCase("SemiTransparentFlyoutPageBackgroundColor", true, true)]
        [TestCase("SemiTransparentFlyoutPageBrush", true, true)]

        public void Issue17022Test(string testButtonID, bool isTopOfScreen, bool requiresScreenshot = false)
        {
            App.WaitForElement(testButtonID).Click();
            var boxView = App.WaitForElement("TopBoxView");
            ClassicAssert.NotNull(boxView);
            var rect = boxView.GetRect();

            try
            {
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