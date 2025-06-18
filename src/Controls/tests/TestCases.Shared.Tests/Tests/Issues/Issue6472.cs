#if TEST_FAILS_ON_WINDOWS //application crash while load the listview, for more information: https://github.com/dotnet/maui/issues/27174
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
		public void ListViewDoesNotThrowExceptionWithObservableCollection()
		{
			App.WaitForElement(ListViewAutomationId);
			App.Tap(ClearButtonAutomationId);
			App.Tap(UiThreadButtonAutomationId);
			App.WaitForElement("Just three");
			App.Tap(OtherThreadButtonAutomationId);
			App.WaitForElement("THE answer");
			App.Tap(ClearButtonAutomationId);
			App.WaitForNoElement("Just three");
			App.WaitForNoElement("THE answer");
			App.Tap(OtherThreadButtonAutomationId);
			App.WaitForElement("THE answer");
			App.Tap(UiThreadButtonAutomationId);
			App.WaitForElement("Just three");

		}
	}
}
#endif