using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla57114 : IssuesUITest
	{
		const string Testing = "Testing...";
		const string Success = "Success";
		const string ViewAutomationId = "_57114View";

		public Bugzilla57114(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Forms gestures are not supported on UIViews that have native gestures";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void _57114BothTypesOfGesturesFire()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement(Testing);
			RunningApp.Tap(ViewAutomationId);
			RunningApp.WaitForNoElement(Success);
		}
	}
}