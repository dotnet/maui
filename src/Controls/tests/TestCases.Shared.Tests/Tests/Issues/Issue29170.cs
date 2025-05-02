#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue29170 : _IssuesUITest
{
	public Issue29170(TestDevice device) : base(device) { }

	public override string Issue => "First Item in CollectionView Overlaps in FlyoutPage.Flyout on iOS";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void CollectionViewFirstItemShouldNotOverlapWithSafeAreaInFlyoutMenu()
	{
		Assert.That(App.WaitForElement("CollectionViewDetail").GetRect().Y, Is.GreaterThanOrEqualTo(54), 
            "CollectionView Y position should be at least 54 to avoid overlapping with safe area");
		App.WaitForElement("FlyoutButton").Tap();
		Assert.That(App.WaitForElement("CollectionViewFlyout").GetRect().Y, Is.GreaterThanOrEqualTo(54), 
            "CollectionView Y position should be at least 54 to avoid overlapping with safe area");
	}
}

//need to wait for test case to complete and then add here for flyout detail page also assert condition and then commit.
//also modoify the test value to 54
#endif
