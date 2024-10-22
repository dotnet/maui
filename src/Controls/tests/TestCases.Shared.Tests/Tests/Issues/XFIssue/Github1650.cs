#if MACCATALYST
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Github1650 : _IssuesUITest
{
	public Github1650(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] Completed event of Entry raised on Tab key";

	// [Test]
	// [Category(UITestCategories.Entry)]
	// public void GitHub1650Test()
	// {
	// 	RunningApp.WaitForElement(q => q.Marked("CompletedTargetEntry"));
	// 	RunningApp.Tap(q => q.Marked("CompletedTargetEntry"));

	// 	Assert.AreEqual(0, _completedCount, "Completed should not have been fired");

	// 	RunningApp.PressEnter();

	// 	Assert.AreEqual(1, _completedCount, "Completed should have been fired once");
	// }
}
#endif