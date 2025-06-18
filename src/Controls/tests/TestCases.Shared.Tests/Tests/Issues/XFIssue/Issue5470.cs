#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
// This test case is specifically for iOS. There's a known framework-level issue with AppLinkEntry on iOS.
// The test is currently disabled on all platforms due to this limitation.
// For more details, see: https://github.com/dotnet/maui/issues/12295
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5470 : _IssuesUITest
{
	public Issue5470(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ApplinkEntry Thumbnail required after upgrading to 3.5/3.6";

	[Test]
	[Category(UITestCategories.AppLinks)]
	public void Issue5470Test()
	{
		App.WaitForElement("IssuePageLabel");
	}
}
#endif