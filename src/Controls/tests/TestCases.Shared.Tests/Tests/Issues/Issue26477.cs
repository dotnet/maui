using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26477 : _IssuesUITest
	{
		public Issue26477(TestDevice device) : base(device) { }

		public override string Issue => "SearchHandler Fails to Display Results on Windows";

		[Test]
		[Category(UITestCategories.Shell)]
		public void VerifySearchHandlerItemsAreVisible()
		{
			App.WaitForElement("Label");
#if IOS || MACCATALYST
			App.EnterText(AppiumQuery.ByXPath("//XCUIElementTypeSearchField"), "Hello");
#elif WINDOWS
			App.EnterText("TextBox", "Hello");
#else
			App.EnterText(AppiumQuery.ByXPath("//android.widget.EditText"), "Hello");
#endif
			VerifyScreenshot();

		}
	}
}