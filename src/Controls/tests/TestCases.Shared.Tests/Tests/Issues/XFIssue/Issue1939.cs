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
		// "Group #1" is cleared by the HostApp ~1.7s after appearing (the operation under test),
		// so waiting on it races with that teardown. Anchor on the post-clear marker instead,
		// which deterministically proves the group was cleared without crashing (#36048).
		App.WaitForElement("ClearCompleted");
	}
}