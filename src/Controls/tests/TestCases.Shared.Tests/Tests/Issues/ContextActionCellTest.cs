#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ContextActionCellTest : _IssuesUITest
{
	public ContextActionCellTest(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Table View ContextActionCell Test";

	[Test]
	[Category(UITestCategories.ActivityIndicator)]
	public void VerifyContextActionCell()
	{
		App.WaitForElement("Text Cell");
#if IOS
		App.SwipeRightToLeft("Text Cell", 25, 500);
#elif ANDROID
		App.LongPress("Text Cell");
#endif
		VerifyScreenshot();
	}
}
#endif