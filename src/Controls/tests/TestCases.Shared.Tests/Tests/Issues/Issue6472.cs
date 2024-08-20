#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6472 : _IssuesUITest
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
		[Category(UITestCategories.Compatibility)]
		[Ignore("Fails occasionally on iOS 12 https://github.com/xamarin/Xamarin.Forms/issues/6472")]
		public void ListViewDoesNotThrowExceptionWithObservableCollection()
		{
			App.WaitForElement(ListViewAutomationId);
			App.Screenshot("We got here without an exception while loading the data and data is visible");

			App.Tap(ClearButtonAutomationId);
			App.Tap(UiThreadButtonAutomationId);
			App.Tap(OtherThreadButtonAutomationId);

			App.Tap(ClearButtonAutomationId);
			App.Tap(OtherThreadButtonAutomationId);
			App.Tap(UiThreadButtonAutomationId);
		}
	}
}
#endif