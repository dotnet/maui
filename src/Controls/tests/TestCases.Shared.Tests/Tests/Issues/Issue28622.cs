#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
//https://github.com/dotnet/maui/issues/27946 In Windows, dynamically changing the ItemsLayout does not work.
// https://github.com/dotnet/maui/issues/28678 On iOS and Mac, in Cv2, changing the ItemsLayout throws an exception. In Cv1, the EmptyView overlaps.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28622 : _IssuesUITest
{
	public Issue28622(TestDevice device) : base(device) { }

	public override string Issue => "[Android] CollectionView Header and Footer Do Not Align with Horizontal ItemsLayout When EmptyView is Displayed";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ItemsLayoutShouldRenderProperlyOnEmptyView()
	{
		App.WaitForElement("LayoutButton");
		App.Tap("LayoutButton");
		App.Tap("HeaderButton");
		App.Tap("FooterButton");
		App.Tap("EmptyViewButton");
		VerifyScreenshot();
	}
}
#endif