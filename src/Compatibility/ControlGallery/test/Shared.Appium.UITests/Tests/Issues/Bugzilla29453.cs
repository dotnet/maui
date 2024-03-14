using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla29453 : IssuesUITest
	{
		public Bugzilla29453(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Navigation.PopAsync(false) in Entry.Completed handler => System.ArgumentException";

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Bugzilla29453Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Screenshot("I am at Issue Bugzilla29453");
			RunningApp.WaitForElement("Page1");
			RunningApp.Tap("btnGotoPage2");
			RunningApp.Tap("entryText");
			RunningApp.EnterText("entryText", "XF");

			RunningApp.DismissKeyboard();
			RunningApp.Back();

			// TODO: Implement PressEnter method.
			//RunningApp.PressEnter();
			//RunningApp.WaitForElement("Page1");
		}
	}
}