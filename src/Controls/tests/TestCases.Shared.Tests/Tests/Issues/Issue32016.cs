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
		Assert.That(text!.Length, Is.EqualTo(10), "Text should be exactly 10 characters");
	}
}