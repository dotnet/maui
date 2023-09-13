using System.Drawing;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using OpenQA.Selenium.Appium.MultiTouch;
using TestUtils.Appium.UITests;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16499 : _IssuesUITest
	{
		public Issue16499(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Crash when using NavigationPage.TitleView and Restarting App";

		[Test]
		public void AppDoesntCrashWhenReusingSameTitleView()
		{
			App.WaitForElement("SuccessLabel");
		}
	}
}
