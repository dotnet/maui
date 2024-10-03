#if !MACCATALYST
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
			App.WaitForElement("TextCell");
			App.Tap("TextCell");
			App.WaitForElement("Button");
			App.Tap("Button");
			App.WaitForElement("TextCell");
			App.Tap("TextCell");
			App.WaitForElement("Button");
		}
	}
}
#endif
