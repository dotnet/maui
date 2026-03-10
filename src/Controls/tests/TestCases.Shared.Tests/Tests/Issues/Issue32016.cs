using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32016 : _IssuesUITest
{
	public override string Issue => "iOS 26 MaxLength not enforced on Entry";

	public Issue32016(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryMaxLengthEnforcedOnIOS26()
	{
		App.WaitForElement("TestEntry");

		// Type characters up to MaxLength
		App.Tap("TestEntry");
		App.EnterText("TestEntry", "1234567890x");

		var text = App.FindElement("TestEntry").GetText();
		Assert.That(text, Is.EqualTo("1234567890"), "Text should be '1234567890' - the 'x' should be blocked by MaxLength");
	}
}