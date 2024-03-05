using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue6472 : IssuesUITest
	{
		const string ListViewAutomationId = "TheListview";
		const string ClearButtonAutomationId = "ClearButton";
		const string UiThreadButtonAutomationId = "UiThreadButton";
		const string OtherThreadButtonAutomationId = "OtherThreadButton";

		public Issue6472(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug][iOS] listview / observable collection throwing native error on load";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewDoesNotThrowExceptionWithObservableCollection()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(ListViewAutomationId);
			App.Screenshot("We got here without an exception while loading the data and data is visible");

			App.Click(ClearButtonAutomationId);
			App.Click(UiThreadButtonAutomationId);
			App.Click(OtherThreadButtonAutomationId);

			App.Click(ClearButtonAutomationId);
			App.Click(OtherThreadButtonAutomationId);
			App.Click(UiThreadButtonAutomationId);
		}
	}
}