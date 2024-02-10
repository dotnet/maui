using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20372 : _IssuesUITest
	{
		public override string Issue => "[iOS] HTML Label not applying Bold or Italics on iOS";

		public Issue20372(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.WebView)]
		public void BothHtmlLabelsShouldApplyBoldAndItalicaProperties()
		{
			_ = App.WaitForElement("label1");

			// Both labels should have proper text attributes
			VerifyScreenshot();
		}
	}
}