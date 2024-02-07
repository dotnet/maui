using NUnit.Framework;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19739 : _IssuesUITest
	{
		public Issue19739(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] Pickers reopen when selecting items";

		[Test]
		public void PickersDialogsShouldAlwaysCloseWhenSelectingItem()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS,
				TestDevice.Mac,
				TestDevice.Windows
			});

			_ = App.WaitForElement("listView");


			for (int i=1;i<=3;i++)
			{
				App.Click($"picker{i}");
				((AppiumApp)App).Driver.FindElements(MobileBy.XPath("//android.widget.TextView[@text=\"Item 1\"]")).First().Click();
			}
			
			// Passes when pickers open dialogs and close them after selecting any item
			VerifyScreenshot();
		}
	}
}
