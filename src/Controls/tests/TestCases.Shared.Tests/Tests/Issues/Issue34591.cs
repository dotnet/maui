using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34591 : _IssuesUITest
{
	public Issue34591(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Label with TailTruncation does not render text when initial text is null";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelWithTailTruncationRendersTextAfterUpdate()
	{
		App.WaitForElement("UpdateTextButton");
		App.Tap("UpdateTextButton");

		var rect = App.WaitForElement("ResultLabel").GetRect();
		Assert.That(rect.Height, Is.GreaterThan(0),
			"Label should have non-zero height after setting text from null");
	}
}
