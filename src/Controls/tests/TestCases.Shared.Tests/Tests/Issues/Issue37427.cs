#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST // This regression is specific to the iOS CollectionView handler.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37427 : _IssuesUITest
{
	public Issue37427(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView item content renders with zero width on iOS";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DynamicallyAddedContentRendersAfterCellRealization()
	{
		App.WaitForElement("37427CollectionView");
		App.ScrollDown("37427CollectionView", ScrollStrategy.Gesture, 0.8, 500);
		App.WaitForElement("37427Card10");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
