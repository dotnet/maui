using Microsoft.Maui.AppiumTests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Controls.AppiumTests.Tests.Issues
{
	public class Issue19099 : _IssuesUITest
	{
		public Issue19099(TestDevice device) : base(device)
		{
		}

		public override string Issue => "TapGestureRecognizer no longer works on Button";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void TapGestureWorksOnButton()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, "This test is only for iOS");

			App.WaitForElement("TestButton").Click();

			var result = App.FindElement("TestLabel").GetText();
			Assert.AreEqual("Success", result);
		}
	}
}
