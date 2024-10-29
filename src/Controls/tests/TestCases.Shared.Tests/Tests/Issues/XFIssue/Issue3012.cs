#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3012 : _IssuesUITest
{
	public Issue3012(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] Entry focus / unfocus behavior";

	// [Test]
	// [Category(UITestCategories.Entry)]
	// public void Issue3012Test()
	// {
	// 	App.WaitForElement(q => q.Marked("DumbyEntry"));
	// 	App.Tap(q => q.Marked("DumbyEntry"));
		
	// 	App.WaitForElement(q => q.Marked("FocusTargetEntry"));
	// 	App.Tap(q => q.Marked("FocusTargetEntry"));
	// 	Assert.AreEqual(0, _unfocusedCount, "Unfocused should not have fired");

	// 	App.Tap(q => q.Marked("DumbyEntry"));
	// 	Assert.AreEqual(1, _unfocusedCount, "Unfocused should have been fired once");
	// }
}
#endif