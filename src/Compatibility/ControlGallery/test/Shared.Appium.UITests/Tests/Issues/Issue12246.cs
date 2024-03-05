using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12246 : IssuesUITest
	{
		const string Entry = "Entry";
		const string Password = "Password";
		const string Success = "Success";

		public Issue12246(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] iOS 14 App freezes when password is entered after email";

		[Test]
		[Category(UITestCategories.Entry)]
		public void UnfocusingPasswordDoesNotHang()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(Entry);
			RunningApp.WaitForElement(Password);

			RunningApp.EnterText(Entry, "test");
			RunningApp.DismissKeyboard();
			RunningApp.EnterText(Password, "test");

			RunningApp.Tap(Entry);
			RunningApp.DismissKeyboard();
			RunningApp.WaitForElement(Success);
		}
	}
}