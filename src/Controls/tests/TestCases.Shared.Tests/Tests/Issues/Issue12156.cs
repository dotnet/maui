#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12156 : _IssuesUITest
{
	public Issue12156(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS Page.LargeTitleDisplay does not work on iOS";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void LargeTitleDisplayWorks()
	{
		if (!OperatingSystem.IsMacOSVersionAtLeast(26)) // Issue Link: https://github.com/dotnet/maui/issues/33879
		{
			Assert.Ignore("Ignored the test on iOS if it runs on macOS version less than 26 due to known visual differences");
		}
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
#endif