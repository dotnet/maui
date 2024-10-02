using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25067 : _IssuesUITest
{
	public Issue25067(TestDevice device) : base(device) { }

	public override string Issue => "Render default icons in SearchHandler";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerRendersDefaultSearchIconAndClearIcon()
	{
		// App.WaitForElement("searchHandler"); doesnt work
		App.WaitForElement("WaitForStubControl");

		VerifyScreenshot();
	}
}