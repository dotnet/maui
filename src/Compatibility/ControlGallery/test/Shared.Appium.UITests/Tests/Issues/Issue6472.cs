#if IOS
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
		[Ignore("Fails occasionally on iOS 12 https://github.com/xamarin/Xamarin.Forms/issues/6472")]
		public void ListViewDoesNotThrowExceptionWithObservableCollection()
		{
			RunningApp.WaitForElement(ListViewAutomationId);
			RunningApp.Screenshot("We got here without an exception while loading the data and data is visible");

			RunningApp.Tap(ClearButtonAutomationId);
			RunningApp.Tap(UiThreadButtonAutomationId);
			RunningApp.Tap(OtherThreadButtonAutomationId);

			RunningApp.Tap(ClearButtonAutomationId);
			RunningApp.Tap(OtherThreadButtonAutomationId);
			RunningApp.Tap(UiThreadButtonAutomationId);
		}
	}
}
#endif