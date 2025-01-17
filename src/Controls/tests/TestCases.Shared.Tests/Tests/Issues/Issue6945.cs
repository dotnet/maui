#if TEST_FAILS_ON_WINDOWS //BoxView automation ID isn't working on the Windows platform, causing a TimeoutException.
//Issue Link: https://github.com/dotnet/maui/issues/27195
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
		public void WrongTranslationBehaviorWhenChangingHeightRequestAndSettingAnchor()
		{
			var rect = App.WaitForElement(BoxViewId).GetRect();
			App.Tap(ClickMeId);
			var rect2 = App.WaitForElement(BoxViewId).GetRect();

			Assert.That(rect.X, Is.EqualTo(rect2.X));
			Assert.That(rect.Y, Is.EqualTo(rect2.Y));
		}
	}
}
#endif