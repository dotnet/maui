using System.Threading;
using Newtonsoft.Json.Bson;
using NUnit.Framework;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue14829 : _IssuesUITest
	{
		public Issue14829(TestDevice device) : base(device)
		{
		}

		public override string Issue => "DisplayActionSheet still not working on Windows";

		[Test]
		[Category(UITestCategories.ActionSheet)]
		public void Issue14829Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.iOS }, "Only affects Windows.");

			App.WaitForElement("DisplayActionSheetButton", timeout: TimeSpan.FromSeconds(4)).Click();
			App.WaitForElementByName("WaitForActionSheet", timeout: TimeSpan.FromSeconds(4));

			VerifyScreenshot();
		}
	}
}
