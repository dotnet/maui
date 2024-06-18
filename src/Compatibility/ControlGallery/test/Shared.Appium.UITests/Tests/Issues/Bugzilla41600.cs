#if ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla41600 : IssuesUITest
	{
		const string BtnScrollToNonExistentItem = "btnScrollToNonExistentItem";
		const string BtnScrollToExistentItem = "btnScrollToExistentItem";
		const string FirstListItem = "0";
		const string MiddleListItem = "15";

		public Bugzilla41600(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Invalid item param value for ScrollTo throws an error";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla41600Test()
		{
			RunningApp.WaitForElement(BtnScrollToNonExistentItem);
			RunningApp.WaitForElement(BtnScrollToExistentItem);

			RunningApp.Tap(BtnScrollToNonExistentItem);
			RunningApp.WaitForNoElement(FirstListItem);

			RunningApp.Tap(BtnScrollToExistentItem);
			RunningApp.WaitForNoElement(MiddleListItem);
		}
	}
}
#endif