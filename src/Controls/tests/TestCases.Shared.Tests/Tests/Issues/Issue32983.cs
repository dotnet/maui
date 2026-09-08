// This test is iOS-only: The test sample uses UISheetPresentationController with a custom detent to present a bottom sheet sized to the measured content height. This API is not available on Android or Windows, and on MacCatalyst it ignores custom detents and always presents as a full-height modal.
#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32983 : _IssuesUITest
{
	public override string Issue => "CollectionView messes up Measure operation on Views";

	public Issue32983(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void BottomSheetDetentHeightIsCorrectWhenCollectionViewIsMeasuredBeforeMount()
	{
		App.WaitForElement("ShowBottomSheetButton");
		App.Tap("ShowBottomSheetButton");

		App.WaitForElement("BottomSheetCollectionView");
		VerifyScreenshot();
	}
}
#endif
