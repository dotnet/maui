using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1939 : _IssuesUITest
{
	public Issue1939(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ArgumentOutOfRangeException on clearing a group on a grouped ListView on Android";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue1939Test()
	{
		// The HostApp clears the "Group #1" items ~1.7s after the page appears, which is the
		// operation under test. Waiting on a group element would race with that clearing, so we
		// instead wait on the stable instructions label. If the clear crashes the app, this
		// WaitForElement call fails and the test fails.
		App.WaitForElement("InstructionsLabel");
	}
}