
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26857 : _IssuesUITest
	{
		public override string Issue => "ListView ScrollTo position always remains at the start even when set to Center or End";

		public Issue26857(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ListView)]
		public void ScrollToSelectedItem26857()
		{
			App.WaitForElement("DownButton");
			App.Tap("DownButton");
			var downLabel = App.WaitForElement("SelectedItemLabel");
			Assert.That(downLabel.GetText(), Is.EqualTo("Item 5"));
			App.WaitForElement("UpButton");
			App.Tap("UpButton");
			var upLabel = App.WaitForElement("SelectedItemLabel");
			Assert.That(upLabel.GetText(), Is.EqualTo("Item 3"));
		}
	}
}
