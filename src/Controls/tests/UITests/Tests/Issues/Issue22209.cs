using System.Drawing;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue22209 : _IssuesUITest
	{
		public Issue22209(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Windows] FlexLayout crashes when using a custom display scale factor value";

		[Test]
		[Category(UITestCategories.Layout)]
		public void FlexLayoutResizeWindow()
		{
			this.IgnoreIfPlatforms(new[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac });

			var app = App as AppiumApp;

			if (app is null)
				return;

			if (app.Driver is not WindowsDriver windowsDriver)
				return;

			Random rnd = new Random();
			for (int i = 0; i < 10; i++)
			{
				windowsDriver.Manage().Window.Size = new Size(rnd.Next(500, 5000), rnd.Next(500, 5000));
			}

			App.WaitForElement("WaitForStubControl");

			// Without exceptions, the test has passed.
		}
	}
}