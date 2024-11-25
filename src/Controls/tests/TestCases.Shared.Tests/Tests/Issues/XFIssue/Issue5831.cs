using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5831 : _IssuesUITest
{
	public Issue5831(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Navigating away from CollectionView and coming back leaves weird old items";

	// TODO: This test was marked ad manual review, can we somehow automate this?
	// TODO: TapInFlyout was some helper method in ControlGallery? Do we have that here?
	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void CollectionViewRenderingWhenLeavingAndReturningViaFlyout()
	//{
	//	TapInFlyout(flyoutOtherTitle);
	//	TapInFlyout(flyoutMainTitle);
	//}
}