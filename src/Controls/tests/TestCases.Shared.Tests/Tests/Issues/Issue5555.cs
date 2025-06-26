using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.TableView)]
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