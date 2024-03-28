using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue6945 : IssuesUITest
	{
		const string ClickMeId = "ClickMeAutomationId";
		const string BoxViewId = "BoxViewAutomationId";

		public Issue6945(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Wrong anchor behavior when setting HeightRequest ";

		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnIOS]
		public void WrongTranslationBehaviorWhenChangingHeightRequestAndSettingAnchor()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			var rect = RunningApp.WaitForElement(BoxViewId).GetRect();
			RunningApp.Tap(ClickMeId);
			var rect2 = RunningApp.WaitForElement(BoxViewId).GetRect();

			ClassicAssert.AreEqual(rect.X, rect2.X);
			ClassicAssert.AreEqual(rect.Y, rect2.Y);
		}
	}
}