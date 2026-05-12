#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23329 : _IssuesUITest
{
	public Issue23329(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Entry select all text on refocus does not work on WinUI";

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntrySelectAllOnRefocusWorks()
	{
		App.WaitForElement("TextBox");
		App.Click("TextBox");
		App.EnterText("TextBox", "Hello World");

		App.Click("OtherElement");
		App.Click("TextBox");

		App.Click("OtherElement");
		App.Click("TextBox");

		VerifyScreenshot();
	}
}
#endif
