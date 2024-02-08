using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue5555 : _IssuesUITest
	{
		public override string Issue => "Memory leak when SwitchCell or EntryCell";
		public Issue5555(TestDevice device) : base(device)
		{
		}

		[Test]
		public void Issue5555Test()
		{
			App.Click("Push page");
			App.WaitForElement("Push page");
			App.Click("Push page");
			App.WaitForElement("Push page");

			App.WaitForElement("You can check result");
			App.Click("Check Result");

			App.WaitForElement("Success");
		}
	}
}