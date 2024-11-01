using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8526 : _IssuesUITest
{
	public Issue8526(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] DisplayPromptAsync hangs app, doesn't display when called in page load";

	//[Test]
	//[Category(UITestCategories.DisplayPrompt)]
	//[FailsOnIOS]
	//public void DisplayPromptShouldWorkInPageLoad()
	//{
	//	App.WaitForElement(Success);
	//	App.Tap("Cancel");
	//}
}