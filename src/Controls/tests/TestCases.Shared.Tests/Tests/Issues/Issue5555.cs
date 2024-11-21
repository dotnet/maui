using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5555 : _IssuesUITest
	{
		public override string Issue => "Memory leak when SwitchCell or EntryCell";
		public Issue5555(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.TableView)]
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void TableViewMemoryLeakWhenUsingSwitchCellOrEntryCell()
		{
			App.WaitForElement("PushPage");
			App.Tap("PushPage");
			App.WaitForElement("PushPage");
			App.Tap("PushPage");
			App.WaitForElement("PushPage");

			App.WaitForElement("CheckResult");
			App.Tap("CheckResult");

			App.WaitForElement("SuccessLabel");
		}
	}
}