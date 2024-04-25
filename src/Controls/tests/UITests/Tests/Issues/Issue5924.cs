using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue5924 : _IssuesUITest
	{
		public override string Issue => "TableView ViewCell vanishes after content is updated";
		public Issue5924(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void TableViewViewCellVanishesAfterContentIsUpdated()
		{
			App.WaitForElement("entry");
			App.EnterText("entry", "I haven't disappeared");

			var entry = App.WaitForElement("entry").GetRect();
			var label = App.WaitForElement("label").GetRect();

			Assert.Greater(entry.Height, 0);
			Assert.Greater(entry.Width, 0);
			Assert.Greater(label.Height, 0);
			Assert.Greater(label.Width, 0);
		}
	}
}