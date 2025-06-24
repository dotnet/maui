#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
// Related issue link https://github.com/dotnet/maui/pull/25642#discussion_r1833637157 - Windows
// The screenshot looks different on iOS depending on using the new or old handler. https://github.com/dotnet/maui/pull/29881#issuecomment-2957208107 - iOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25598 : _IssuesUITest
	{
		public Issue25598(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "IndicatorView with Template won't show when ItemSource reaches 0 Elements";

		[Test]
		[Category(UITestCategories.IndicatorView)]
		public void IndicatorWithTemplateShouldBeVisible()
		{
			App.WaitForElement("RemoveItemButton");

			for (int i = 0; i < 3; i++)
				App.Click("RemoveItemButton");

			for (int i = 0; i < 3; i++)
				App.Click("AddItemButton");

			VerifyScreenshot();
		}
	}
}
#endif