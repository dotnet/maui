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
		[Category(UITestCategories.Entry)]
		public void HittingEnterKeySendsDone()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac,
				TestDevice.iOS,
				TestDevice.Windows,
			});

			App.Tap("HardwareEnterKeyEntry");
			App.SendKeys(66);
			App.WaitForElement("Success");
		}
	}
}
