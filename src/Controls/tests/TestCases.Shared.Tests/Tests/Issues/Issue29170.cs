#if IOS //iOS only support the SetUseSafeArea
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
		// Use 44 (status bar height) as conservative minimum instead of device-specific value
		// This prevents test flakiness across different simulators and iOS versions
		const int MinimumSafeAreaHeight = 44;
		
		Assert.That(App.WaitForElement("CollectionViewDetail").GetRect().Y, Is.GreaterThanOrEqualTo(MinimumSafeAreaHeight), 
            $"CollectionView Y position should be at least {MinimumSafeAreaHeight} to avoid overlapping with safe area");
		App.WaitForElement("FlyoutButton").Tap();
		Assert.That(App.WaitForElement("CollectionViewFlyout").GetRect().Y, Is.GreaterThanOrEqualTo(MinimumSafeAreaHeight), 
            $"CollectionView Y position should be at least {MinimumSafeAreaHeight} to avoid overlapping with safe area");
	}
}
#endif
