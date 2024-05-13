using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18617 : _IssuesUITest
	{
		public Issue18617(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Button Command CanExecute can disable the control";

		[Test]
		[Category(UITestCategories.Button)]
		public void CommandCanExecuteDisableButton()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Press the 'On' button.
			App.Tap("OnButton");

			// 2. The test fails if the 'On' button is still enabled or the 'Off' button is not enabled.
			var status1 = App.FindElement("StatusLabel").GetText();
			Assert.AreEqual("OnButton disabled OffButton enabled", status1);

			// 3.Press the 'Off' button.
			App.Tap("OffButton");

			// 4. The test fails if the 'Off' button is still enabled or the 'On' button is not enabled.
			var status2 = App.FindElement("StatusLabel").GetText();
			Assert.AreEqual("OnButton enabled OffButton disabled", status2);

			// NOTE: We do not verify snapshots because we already did it in G1.
		}
	}
}
