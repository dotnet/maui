using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18371 : _IssuesUITest
{
	public Issue18371(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell navigation bar color should be ready during modal dismissal";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void NavigationBarColorIsPreservedAfterReturningFromModal()
	{
		App.WaitForElement("OpenWelcome");
		App.Tap("OpenWelcome");
		App.WaitForElement("ReturnHome");
		App.Tap("ReturnHome");
		App.WaitForElement("OpenWelcome");

		VerifyScreenshot();
	}
}
