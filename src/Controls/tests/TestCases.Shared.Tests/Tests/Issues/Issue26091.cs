using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26091 : _IssuesUITest
	{
		public override string Issue => "TableView cells become empty after adding a new cell with context actions";

		public Issue26091(TestDevice device)
		: base(device)
		{ }

		string item = "Add new";

		[Test]
		[Category(UITestCategories.TableView)]
		public void DynamicallyAddingTableViewCells()
		{
			App.WaitForElement("TableView");
			App.Tap(item);
			App.Tap(item);
			App.Tap(item);

			VerifyScreenshot();
		}
	}
}