#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12079 : _IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";

		public Issue12079(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SwipeView crash if Text not is set on SwipeItem";

		[Test]
		[Ignore("Appium cannot find the SwipeControl, we have to review the reason.")]
		[Category(UITestCategories.SwipeView)]
		[Category(UITestCategories.Compatibility)]
		public void SwipeItemNoTextWindows()
		{
			App.WaitForElement(SwipeViewId);
			App.SwipeLeftToRight(SwipeViewId);
			App.Tap(SwipeViewId);
			App.WaitForNoElement("Success");
			App.Screenshot("The test has passed");
		}
	}
}
#endif