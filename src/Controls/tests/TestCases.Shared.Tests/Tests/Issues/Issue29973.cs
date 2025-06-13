using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29973 : _IssuesUITest
{
	public override string Issue => "[Android] SearchHandler default icons are not displayed correctly";

	public Issue29973(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifySearchHandlerDefaultIcons()
	{
		App.WaitForElement("valueButton");
		App.Tap("valueButton");
		VerifyScreenshot();
	}
}