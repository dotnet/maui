using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7906 : _IssuesUITest
{
	public override string Issue => "Entry and Editor: option to customize underline";

	public Issue7906(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Entry)]
	public void UnderlineColorPropertyWorks()
	{
		// Wait for first visible element with extended timeout
		App.WaitForElement("DefaultEntry", timeout: TimeSpan.FromSeconds(30));

		// Verify we can still find elements after interaction
		var entry = App.FindElement("DefaultEntry");
		Assert.That(entry, Is.Not.Null, "Entry should remain accessible");
		VerifyScreenshot();
	}
}
