using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28975 : _IssuesUITest
{
	public Issue28975(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Setting the Entry keyboard type to Date results in it behaving like a password entry";

	[Test]
	[Category(UITestCategories.Entry)]
	public void EnsureEntryWithDateKeyboardIsNotPassword()
	{
		App.WaitForElement("DateEntry");
		App.EnterText("DateEntry", "2023-10-01");
		VerifyScreenshot();
	}
}