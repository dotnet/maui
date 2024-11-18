using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7339 : _IssuesUITest
{
	public Issue7339(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Material frame renderer not being cleared";

	// TODO: TapInFlyout was some helper method in ControlGallery? Do we have that here?
	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void MaterialFrameDisposesCorrectly()
	//{
	//	TapInFlyout("Item1");
	//	TapInFlyout("Item2");
	//	TapInFlyout("Item1");
	//	TapInFlyout("Item2");
	//}
}