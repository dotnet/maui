using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19997 : _IssuesUITest
	{
		public override string Issue => "[iOS] Facing issues with the Entry, DatePicker, and CheckBox components when applying themes ";

		public Issue19997(TestDevice device) : base(device) { }

		[Test]
		public void DarkThemeShouldBeApplied()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows }, "This is an iOS theme change issue.");

			App.WaitForElement("entry");
			App.Click("entry");

			//The Test passes if keyboard is displayed in the dark theme
			VerifyScreenshot();
		}
	}
}
