#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6945 : _IssuesUITest
	{
		const string ClickMeId = "ClickMeAutomationId";
		const string BoxViewId = "BoxViewAutomationId";

		public Issue6945(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Wrong anchor behavior when setting HeightRequest ";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void WrongTranslationBehaviorWhenChangingHeightRequestAndSettingAnchor()
		{
			var rect = App.WaitForElement(BoxViewId).GetRect();
			App.Tap(ClickMeId);
			var rect2 = App.WaitForElement(BoxViewId).GetRect();

			ClassicAssert.AreEqual(rect.X, rect2.X);
			ClassicAssert.AreEqual(rect.Y, rect2.Y);
		}
	}
}
#endif