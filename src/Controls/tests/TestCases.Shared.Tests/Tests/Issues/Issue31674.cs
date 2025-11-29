using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31674 : _IssuesUITest
	{
		public override string Issue => "Label with TextType Html the label is measured as height 0";

		public Issue31674(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Label)]
		public void HtmlLabelShouldBeVisibleWithInfiniteHeightMeasure()
		{
			// Wait for the HTML label to be ready
			App.WaitForElement("HtmlLabel");

			// Get the text from the HTML label
			var htmlLabelText = App.FindElement("HtmlLabel").GetText();

			// Verify the HTML label has text (not empty)
			Assert.That(htmlLabelText, Is.Not.Null);
			Assert.That(htmlLabelText, Is.Not.Empty);
			Assert.That(htmlLabelText!.ToLower(System.Globalization.CultureInfo.InvariantCulture), Does.Contain("hello"));

			// Get the size of the HTML label to ensure it's not height 0
			var htmlLabelRect = App.FindElement("HtmlLabel").GetRect();
			Assert.That(htmlLabelRect.Height, Is.GreaterThan(0), "HTML label should have height > 0");

			// For comparison, verify plain text label also works
			App.WaitForElement("PlainLabel");
			var plainLabelText = App.FindElement("PlainLabel").GetText();
			Assert.That(plainLabelText, Is.Not.Null);
			Assert.That(plainLabelText, Is.Not.Empty);
			
			var plainLabelRect = App.FindElement("PlainLabel").GetRect();
			Assert.That(plainLabelRect.Height, Is.GreaterThan(0), "Plain label should have height > 0");
		}
	}
}
