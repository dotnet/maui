#if TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/29493
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35088 : _IssuesUITest
{
	public override string Issue => "SearchHandler.BackgroundColor cannot be reset to null";

	public Issue35088(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerBackgroundColorResetsToDefault()
	{
		App.WaitForElement("ResetColorButton");
		App.Tap("ResetColorButton");

		// Take screenshot verifying the background returned to default
		VerifyScreenshot();
	}
}
#endif
