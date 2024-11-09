using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue4684 : _IssuesUITest
{
	public Issue4684(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] don't clear shell content because native page isn't visible";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void NavigatingBackAndForthDoesNotCrash()
	//{
	//	TapInFlyout("Connect");
	//	App.Tap("Control");

	//	TapInFlyout("Home");
	//	TapInFlyout("Connect");

	//	App.Tap("Connect");
	//	App.Tap("Control");

	//	App.WaitForElement("Success");
	//}
}