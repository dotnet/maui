using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19331 : _IssuesUITest
{
	public Issue19331(TestDevice device) : base(device) { }

	public override string Issue => "SwipeItems referencing causes crash on Android";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SharedSwipeItemsShouldNotCrashWhenSwipingMultipleRows()
	{
		App.WaitForElement("StatusLabel19331");
		App.SwipeLeftToRight("SwipeRow1_19331");
		App.SwipeLeftToRight("SwipeRow2_19331");
		App.WaitForElement("StatusLabel19331");
	}
}
