using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25093 : _IssuesUITest
{
	public Issue25093(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[iOS] TintColor on UIButton image no longer working when button made visible";

	[Test]
	[Category(UITestCategories.Button)]
	public void ButtonImageTintColorPreservedAfterResize()
	{
		App.WaitForElement("ApplyTintButton");
		App.Tap("ApplyTintButton");

		// Wait for the layout cycle and verification to complete
		App.WaitForElement("StatusLabel");

		var statusText = App.WaitForElement("StatusLabel", timeout: TimeSpan.FromSeconds(3)).GetText();

		// The label may still say "Waiting" if SizeChanged hasn't fired yet, so retry
		if (statusText == "Waiting")
		{
			Thread.Sleep(1000);
			statusText = App.FindElement("StatusLabel").GetText();
		}

		Assert.That(statusText, Is.EqualTo("PASS"));
	}
}
