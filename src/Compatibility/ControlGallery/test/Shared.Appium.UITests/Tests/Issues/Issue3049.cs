#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3049 : IssuesUITest
	{
		const string Button1Id = "button1";
		const string Button2Id = "button2";
		const string LabelId = "label";
		const string Success = "Success";
		const string Action1 = "Don't click me";

		public Issue3049(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "DisplayActionSheet freezes app in iOS custom renderer (iPad only)";

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		[FailsOnIOS]
		public async Task Issue3049Test()
		{
			RunningApp.WaitForElement(Button1Id);

			RunningApp.Tap(Button1Id);

			await Task.Delay(500);
			//RunningApp.WaitForElement(Action1);

			// Tap outside ActionSheet to dismiss it
			RunningApp.Tap(LabelId);

			RunningApp.WaitForElement(Button2Id);
			RunningApp.Tap(Button2Id);

			RunningApp.WaitForElement(Success);
		}
	}
}
#endif