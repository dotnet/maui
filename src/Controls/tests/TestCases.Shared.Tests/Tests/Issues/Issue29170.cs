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
		App.WaitForElement("FlyoutButton").Tap();
		Assert.That(App.WaitForElement("CollectionView").GetRect().Y, Is.GreaterThanOrEqualTo(69), 
            "CollectionView Y position should be at least 69 to avoid overlapping with safe area");
	}
}
#endif
