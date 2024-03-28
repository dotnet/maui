using Microsoft.Maui.AppiumTests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Controls.AppiumTests.Tests.Issues
{
	public class Issue19197 : _IssuesUITest
	{
		public Issue19197(TestDevice device) : base(device)
		{
		}

		public override string Issue => "AdaptiveTrigger does not work";

		[Test]
		[Category(UITestCategories.Page)]
		public void AdaptiveTriggerWorks()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, "This test is only for iOS");
		
			App.WaitForElement("WaitForStubControl");
			App.SetOrientationLandscape();
			var text = App.FindElement("WaitForStubControl").GetText();
			Assert.AreEqual("Horizontal", text);
			App.SetOrientationPortrait();
		}
	}
}