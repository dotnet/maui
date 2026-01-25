using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class EntrySelectionTest : _IssuesUITest
{
	public EntrySelectionTest(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Entry select all text issue";

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifySelectWorks()
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