using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19366 : _IssuesUITest
	{
		public Issue19366(TestDevice device) : base(device) { }

		public override string Issue => "Items are enabled when ListView is not enabled";

		[Test]
		public void ListCellsItemsShouldNotBeEnabledWhenListViewIsNotEnabled()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement("button1");

			// 1. Click the button of the first cell and observe the label's text change
			App.Click("button1");
			var listItemOneText = App.FindElement("label1").GetText();
			Assert.True(listItemOneText == "Clicked");

			// 2. Set IsEnabled property of the list to false
			App.Click("disableListButton");

			// 3. Click the button of the second cell and observe the label's text do not change
			App.Click("button2");
			var listItemTwoText = App.FindElement("label2").GetText();
			Assert.True(listItemTwoText == "Not clicked");
		}
	}
}