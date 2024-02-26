using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
    public class Issue10563 : IssuesUITest
	{
		const string OpenLeftId = "OpenLeftId";
		const string OpenRightId = "OpenRightId";
		const string OpenTopId = "OpenTopId";
		const string OpenBottomId = "OpenBottomId";
		const string CloseId = "CloseId";

		public Issue10563(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] SwipeView Open methods does not work for RightItems"; 
		
		[Test]
		public void Issue10563OpenSwipeViewTest()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(OpenLeftId);
			App.Click(OpenLeftId);
			App.Screenshot("Left SwipeItems");
			App.Click(CloseId);

			App.WaitForElement(OpenRightId);
			App.Click(OpenRightId);
			App.Screenshot("Right SwipeItems");

			App.Click(CloseId);

			App.WaitForElement(OpenTopId);
			App.Click(OpenTopId);
			App.Screenshot("Top SwipeItems");
			App.Click(CloseId);

			App.WaitForElement(OpenBottomId);
			App.Click(OpenBottomId);
			App.Screenshot("Bottom SwipeItems");
			App.Click(CloseId);
		}
	}
}
