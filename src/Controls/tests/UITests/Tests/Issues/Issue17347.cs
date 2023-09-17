using System.Drawing;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using OpenQA.Selenium.Appium.MultiTouch;
using TestUtils.Appium.UITests;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue17347 : _IssuesUITest
	{
		public Issue17347(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting a new TitleView on an already created page crashes iOS";

		[Test]
		public void AppDoesntCrashWhenSettingNewTitleViewOnExistingPage()
		{
			try
			{
				App.WaitForElement("TitleViewLabel4", timeout: TimeSpan.FromSeconds(4));
			}
			finally
			{
				App.Tap("PopMeButton");
			}
		}
	}
}
