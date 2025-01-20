#if !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23801 : _IssuesUITest
	{
		public override string Issue => "Span GestureRecognizers don't work when the span is wrapped over two lines";

		public Issue23801(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Label)]
		public void VerifyLabelSpanGestureWhenWrappedOverTwoLines()
		{
			var label = App.WaitForElement("Label");
			var location = label.GetRect();
			var height = location.Height / 2;
			var endOfFirstLine = location.X + location.Width - 150;
			App.Click(endOfFirstLine, location.Y + height);
			var testlabel = App.WaitForElement("TestLabel");
			Assert.That(testlabel.GetText(), Is.EqualTo("Label span tapped"));
		}
	}
}
#endif