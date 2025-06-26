#if IOS // This test case verifies that the UINavigationBar behaves as expected (translucent or transparent) exclusively on the iOS platform.
using Xunit;
using Xunit;
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
		[Fact]
		[Trait("Category", UITestCategories.Navigation)]
		[Theory]
		[InlineData("NewNavigationPageButton", false)]
        [Theory]
		[InlineData("NewNavigationPageTransparentButton", false)]
        [Theory]
		[InlineData("NewNavigationPageTranslucentButton", false)]
        [Theory]
		[InlineData("NewNavigationPageTransparentTranslucentButton", false)]
        [Theory]
		[InlineData("NewNavigationPageGridButton", false)]
        [Theory]
		[InlineData("NewNavigationPageGridTransparentButton", false)]
        [Theory]
		[InlineData("NewNavigationPageGridTranslucentButton", false, true)] // this test thinks the boxview is at the top of the screen, but it's not. Test this case manually for now.
        [Theory]
		[InlineData("NewNavigationPageGridTransparentTranslucentButton", true)]
        [Theory]
		[InlineData("NewFlyoutPageButton", false)]
        [Theory]
		[InlineData("NewFlyoutPageTransparentButton", false)]
        [Theory]
		[InlineData("NewFlyoutPageTranslucentButton", false)]
        [Theory]
		[InlineData("NewFlyoutPageTransparentTranslucentButton", false)]
        [Theory]
		[InlineData("NewFlyoutPageGridButton", false)]
        [Theory]
		[InlineData("NewFlyoutPageGridTransparentButton", false)]
        [Theory]
		[InlineData("NewFlyoutPageGridTranslucentButton", false, true)] // this test thinks the boxview is at the top of the screen, but it's not. Test this case manually for now.
        [Theory]
		[InlineData("NewFlyoutPageGridTransparentTranslucentButton", true)]
        [Theory]
		[InlineData("SemiTransparentNavigationPageBackgroundColor", true, true)]
        [Theory]
		[InlineData("SemiTransparentNavigationPageBrush", true, true)]
        [Theory]
		[InlineData("SemiTransparentFlyoutPageBackgroundColor", true, true)]
        [Theory]
		[InlineData("SemiTransparentFlyoutPageBrush", true, true)]

		public void Issue17022Test(string testButtonID, bool isTopOfScreen, bool requiresScreenshot = false)
		{
            App.WaitForElement(testButtonID).Click();
            var boxView = App.WaitForElement("TopBoxView");
            Assert.NotNull(boxView);
			var rect = boxView.GetRect();

            try { 
                if (requiresScreenshot)
                {
                    VerifyScreenshot(GetCurrentTestName() + testButtonID);
                }
                else
                {
                    if (isTopOfScreen)
                    {
                        Assert.Equal(rect.Y, 0);
                    }
                    else
                    {
						Assert.NotEqual(rect.Y, 0);
                    }
                }
            }
            catch 
            { 
                throw new InvalidOperationException("Failed with exception");
            }
            finally
            {
                App.WaitForElement("PopPageButton").Click();
            }
		}
	}
}
#endif