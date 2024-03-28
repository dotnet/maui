using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20372 : _IssuesUITest
	{
		public override string Issue => "[iOS] HTML Label not applying Bold or Italics on iOS";

		public Issue20372(TestDevice device) : base(device)
		{
		}

		[Test]
		public void BothHtmlLabelsShouldApplyBoldAndItalicaProperties()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows, TestDevice.Android });

			_ = App.WaitForElement("label1");

			// Both labels should have proper text attributes
			VerifyScreenshot();
		}
	}
}
