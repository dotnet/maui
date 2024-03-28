using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20166 : _IssuesUITest
	{
		public Issue20166(TestDevice device) : base(device) { }

		public override string Issue => "Custom FlyoutIcon visible although FlyoutBehavior set to disabled";

		[Test]
		public void ShouldHideCustomFlyoutIconWhenNavigatingToPageWithDisabledFlyout()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			// Click button 1 to switch to the page with disabled flyout
			App.WaitForElement("button1");
			App.Click("button1");

			// 2. Verify that the flyout icon is not rendered.
			VerifyScreenshot();
		}
	}
}