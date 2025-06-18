using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21112 : _IssuesUITest
	{
		public Issue21112(TestDevice testDevice) : base(testDevice)
		{
		}
		public override string Issue => "TableView TextCell command executes only once";

		[Test]
		[Category(UITestCategories.TableView)]
		public void TableViewTextCellCommand()
		{
			App.WaitForElement("MainPageButton");
			App.Tap("MainPageButton");
			App.WaitForElement("NavigatedPageButton");
			App.Tap("NavigatedPageButton");
			App.WaitForElement("MainPageButton");
			App.Tap("MainPageButton");
			var label = App.WaitForElement("NavigatedPageLabel");
			Assert.That(label.GetText(), Is.EqualTo("Main Page"));
		}
	}
}