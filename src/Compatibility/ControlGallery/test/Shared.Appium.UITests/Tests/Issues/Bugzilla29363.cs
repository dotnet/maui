using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla29363 : IssuesUITest
	{
		public Bugzilla29363(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PushModal followed immediate by PopModal crashes";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void PushButton()
		{
			RunningApp.WaitForElement("ModalPushPopTest");
			RunningApp.Tap("ModalPushPopTest");
			Thread.Sleep(2000);

			// if it didn't crash, yay
			RunningApp.WaitForElement("ModalPushPopTest");
		}
	}
}