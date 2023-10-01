using System.Drawing;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using OpenQA.Selenium.Appium.MultiTouch;
using TestUtils.Appium.UITests;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17490 : _IssuesUITest
	{
		public Issue17490(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Crash using Pinvoke.SetParent to create Window as Child";

		[Test]
		public void AppDoesntCrashWhenOpeningWinUIWindowParentedToCurrentWindow()
		{
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.iOS, TestDevice.Android
			});

			App.WaitForElement("SuccessLabel");
		}
	}
}
