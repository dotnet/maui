using NUnit.Framework;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16386 : _IssuesUITest
	{
		public Issue16386(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Process the hardware enter key as \"Done\"";

		[Test]
		public void HittingEnterKeySendsDone()
		{
			App.Click("HardwareEnterKeyEntry");
			
			if (Device == TestDevice.Android || Device == TestDevice.Windows)
			{
				App.SendKeys(66);
			}
			else
			{
				App.WaitForElement("HardwareEnterKeyEntry").SendKeys(Keys.Enter);
			}

			App.WaitForElement("Success");
		}
	}
}
