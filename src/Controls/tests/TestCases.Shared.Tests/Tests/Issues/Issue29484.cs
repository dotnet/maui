#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS
// This test is disabled on mobile platforms because they do not support pointer hover states.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29484 : _IssuesUITest
{
	public Issue29484(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView Selected state does not work on the selected item when combined with PointerOver";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void PointerOverWithSelectedStateShouldWork()
	{
		App.WaitForElement("CollectionView");
		App.Tap("Item 2");
		App.Tap("PointerOverAndSelectedState");
		VerifyScreenshot();
	}
}
#endif