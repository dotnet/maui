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

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void GitHubIssue6184()
	//{
	//	RunningApp.WaitForElement(q => q.Marked("More"));
	//	RunningApp.Tap(q => q.Marked("More"));
	//	RunningApp.Tap(q => q.Marked("Issue 5"));
	//	RunningApp.WaitForElement(q => q.Marked("Issue 5"));
	//}
}