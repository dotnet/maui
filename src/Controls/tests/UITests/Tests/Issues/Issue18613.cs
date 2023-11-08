using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18613 : _IssuesUITest
	{
		public Issue18613(TestDevice device) : base(device) { }

		public override string Issue => "Disabled Button cannot trigger Command";

		[Test]
		public void CommandDisableButton()
		{
			App.WaitForElement("WaitForStubControl");
			var button = App.FindElement("ButtonControl");

			if (button is not null)
			{
				button.Click();

				// If clicking the Button we do not have a counter result in
				// the Label, the command has not been executed.
				var counter = App.FindElement("CounterLabel").GetText();
				Assert.True(string.IsNullOrEmpty(counter));
			}

			// Verify the visual status of the disabled Button.
			VerifyScreenshot();
		}
	}
}
