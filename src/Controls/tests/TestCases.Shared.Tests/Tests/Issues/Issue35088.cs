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

		Exception? exception = null;

		// Verify the custom YellowGreen background is applied
		VerifyScreenshotOrSetException(ref exception, "SearchHandlerBackgroundColorApplied");

		App.Tap("ResetColorButton");

		// Verify the background returned to default
		VerifyScreenshotOrSetException(ref exception);

		if (exception is not null)
		{
			throw exception;
		}
	}
}
#endif
