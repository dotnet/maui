using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28634 : _IssuesUITest
{
	public override string Issue => "[Android] SearchHandler Placeholder Text";

	public Issue28634(TestDevice device)
	: base(device)
	{ }

	[Fact]
	[Trait("Category", UITestCategories.Shell)]
	public void VerifySearchHandlerPlaceholderText()
	{
		App.WaitForElement("button");
		App.Tap("button");
		VerifyScreenshot();
	}
}
