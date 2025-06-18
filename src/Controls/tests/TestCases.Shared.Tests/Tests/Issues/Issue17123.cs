using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17123 : _IssuesUITest
	{
		public override string Issue => "TableSection and TableRoot Title properties are displayed as lower case";

		public Issue17123(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.TableView)]
		public void ValidateTableViewTitles()
		{
			App.WaitForElement("TableView");
			var tableViewRootText = App.FindElement("TableRootLabel").GetText();
			if (string.IsNullOrEmpty(tableViewRootText))
			{
				Assert.Fail("Table root text is does not match expected");
			}
			else
			{
				Assert.That(tableViewRootText, Is.EqualTo("TableRoot Title Text"));
			}

			var tableViewSelectionText = App.FindElement("TableSectionLabel").GetText();
			if (string.IsNullOrEmpty(tableViewSelectionText))
			{
				Assert.Fail("Table selection text is does not match expected");
			}
			else
			{
				Assert.That(tableViewSelectionText, Is.EqualTo("TableSection Title Text"));
			}

			VerifyScreenshot();
		}
	}
}