#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Cell disabled is not behaves correctly except iOS. More Information: https://github.com/dotnet/maui/issues/5161
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6184 : _IssuesUITest
{
	public Issue6184(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Throws exception when set isEnabled to false in ShellItem index > 5";

	[Test]
	[Category(UITestCategories.Shell)]
	public void GitHubIssue6184()
	{
		App.WaitForElement("More");
		App.Tap("More");
		App.Tap("Issue 5");
		App.WaitForElement("Issue 5");
 
	}
}
#endif
