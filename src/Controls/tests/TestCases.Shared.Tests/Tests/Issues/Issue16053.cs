using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16053: _IssuesUITest
	{
		public Issue16053(TestDevice device) : base(device)
		{
		}

		public override string Issue => "ListView SelectedItem retains its value after ListView is cleared";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewSelectedItemUpdated()
		{
			App.WaitForElement("RemoveSelectedItemButton");
			App.Tap("ShowSelectedItem");
			var label = App.WaitForElement("SelectedItemLabel");
			Assert.That(label.GetText(), Is.EqualTo("Tea"));
			App.Tap("ClearButton");
			App.Tap("ShowSelectedItem");
			Assert.That(label.GetText(), Is.EqualTo("null"));
			App.Tap("ChangeItemsButton");
			App.Tap("ShowSelectedItem");
			Assert.That(label.GetText(), Is.EqualTo("Soda"));
			App.Tap("RemoveSelectedItemButton");
			App.Tap("ShowSelectedItem");
			Assert.That(label.GetText(), Is.EqualTo("null"));
		}
	}
}