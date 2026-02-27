#if TEST_FAILS_ON_WINDOWS // On Windows, the ClearPlaceholderIcon is not displayed in the SearchHandler.
// Issue: https://github.com/dotnet/maui/issues/28619
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20250 : _IssuesUITest
{
	public override string Issue => "[iOS] SearchHandler ClearPlaceholderIcon color";

	public Issue20250(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifySearchHandlerClearPlaceholderIconColor()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}

#endif
