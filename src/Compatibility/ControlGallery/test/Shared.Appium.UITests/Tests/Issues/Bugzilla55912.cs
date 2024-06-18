using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla55912 : IssuesUITest
	{
		const string Success = "Success";
		const string GridLabelId = "GridLabel";
		const string StackLabelId = "StackLabel";

		public Bugzilla55912(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Tap event not always propagated to containing Grid/StackLayout";

		[Test]
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Layout)]
		[FailsOnIOS]
		public void GestureBubblingInStackLayout()
		{
			RunningApp.WaitForElement(StackLabelId);
			RunningApp.Tap(StackLabelId);
			RunningApp.WaitForElement(Success);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Layout)]
		[FailsOnIOS]
		public void GestureBubblingInGrid()
		{
			RunningApp.WaitForElement(GridLabelId);
			RunningApp.Tap(GridLabelId);
			RunningApp.WaitForElement(Success);
		}
	}
}