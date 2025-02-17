#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// On iOS and Catalyst BoxView is not rendered, issue: https://github.com/dotnet/maui/issues/18746, 
// On Windows Clip is not properly working, issue: https://github.com/dotnet/maui/issues/14078
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7823_XF : _IssuesUITest
{
	public Issue7823_XF(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Frame corner radius.";

	[Test]
	[Category(UITestCategories.Frame)]
	public void Issue7823TestIsClippedIssue()
	{
		App.WaitForElement("Frame Corner Radius");
		App.Tap("SetClipBounds");
		VerifyScreenshot();
	}
}
#endif