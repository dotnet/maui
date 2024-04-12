using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla36703 : IssuesUITest
	{
		const string TestImage = "testimage";
		const string Success = "Success";
		const string Toggle = "toggle";
		const string Testing = "Testing...";

		public Bugzilla36703(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TapGestureRecognizer inside initially disable Image will never fire Tapped event";

		[Test]
		[Category(UITestCategories.Gestures)]
		[FailsOnIOS]
		public void _36703Test()
		{
			RunningApp.WaitForElement(TestImage);
			RunningApp.Tap(TestImage);
			RunningApp.WaitForNoElement(Testing);
			RunningApp.Tap(Toggle);
			RunningApp.Tap(TestImage);
			RunningApp.WaitForNoElement(Success);
		}
	}
}