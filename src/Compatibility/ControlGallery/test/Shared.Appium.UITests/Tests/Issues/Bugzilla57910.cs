using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla57910 : IssuesUITest
	{
		const string ButtonId = "btnPush";
		const string Button2Id = "btnPop";

		public Bugzilla57910(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";
		public override bool ResetMainPage => false;

		// Crash after navigation
		/*
		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Navigation)]
		[FailsOnAndroid]
		public void Bugzilla57910Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			for (int i = 0; i < 10; i++)
			{
				RunningApp.WaitForElement(ButtonId);
				RunningApp.Tap(ButtonId);
				RunningApp.WaitForElement(Button2Id);
				RunningApp.Tap(Button2Id);
			}
		}
		*/
	}
}