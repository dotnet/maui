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
		public async Task Issue3049Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(Button1Id);

			App.Click(Button1Id);

			await Task.Delay(500);
			//App.WaitForElement(Action1);

			// Tap outside ActionSheet to dismiss it
			App.Click(LabelId);

			App.WaitForElement(Button2Id);
			App.Click(Button2Id);

			App.WaitForElement(Success);
		}
	}
}