using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25983 : _IssuesUITest
	{
		public Issue25983(TestDevice device) : base(device) { }

		public override string Issue => "Issue25983 Grid not getting invalidated when changing the Height/Width of Row/ColumnDefinitions declared with the short syntax";

		[Test]
		[Category(UITestCategories.Layout)]
		public void VerifyGridRowAndColumnSizeInvalidatedCorrectly()
		{
			var resizeRowButton = App.WaitForElement("resizeRowButton");
			var resizeColumnButton = App.WaitForElement("resizeColumnButton");

			resizeRowButton.Click();
			resizeColumnButton.Click();
			Task.Delay(500).Wait();

			VerifyScreenshot();
		}
	}
}
