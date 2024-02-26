using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue11333 : IssuesUITest
	{
		const string SwipeViewId = "SwipeViewId";

		public Issue11333(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] SwipeView does not work on Android if child has TapGestureRecognizer";

		[Test]
		public void SwipeWithChildGestureRecognizer()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(SwipeViewId);
			App.SwipeRightToLeft();
			App.Click(SwipeViewId);
			App.WaitForElement("ResultLabel");
		}
	}
}
