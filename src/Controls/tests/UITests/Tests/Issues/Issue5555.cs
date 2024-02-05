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
		public void TableViewMemoryLeakWhenUsingSwitchCellOrEntryCell()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac,
				TestDevice.iOS,
			});

			App.WaitForElement("PushPage");
			App.Click("PushPage");
			App.WaitForElement("PushPage");
			App.Click("PushPage");
			App.WaitForElement("PushPage");

			App.WaitForElement("CheckResult");
			App.Click("CheckResult");

			App.WaitForElement("SuccessLabel");
		}
	}
}