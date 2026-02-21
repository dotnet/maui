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
		public void TableViewMemoryLeakWhenUsingSwitchCellOrEntryCell()
		{
			// Wait for NavigationPage to fully load its content
			// The Issue5555 page is a NavigationPage wrapping TestPage - give it time to initialize
			App.WaitForElement("PushPage", timeout: TimeSpan.FromSeconds(30));
			App.Tap("PushPage");
			
			// Wait for push/pop animation to complete - need extra delay for navigation animation
			Thread.Sleep(500);
			App.WaitForElement("PushPage", timeout: TimeSpan.FromSeconds(10));
			App.Tap("PushPage");
			
			// Wait for second push/pop animation to complete  
			Thread.Sleep(500);
			App.WaitForElement("PushPage", timeout: TimeSpan.FromSeconds(10));

			App.WaitForElement("CheckResult");
			App.Tap("CheckResult");

			App.WaitForElement("SuccessLabel");
		}
	}
}