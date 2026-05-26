using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21640 : _IssuesUITest
	{
		public Issue21640(TestDevice testDevice) : base(testDevice)
		{
		}
		public override string Issue => "TabbedPage content was not updated";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void TabbedPageContentUpdatesCorrectly()
		{
#if ANDROID
			string tab1Title = "HOME";
			string tab2Title = "SETTINGS";
#else
			string tab1Title = "Home";
			string tab2Title = "Settings";
#endif
			string toggleTabButtonId = "ToogleTabButton";
			App.WaitForElement(toggleTabButtonId);
			App.Tap(toggleTabButtonId);
			App.Tap(tab2Title);
			App.WaitForElement("label");

			// The issue occurs when repeatedly removing and re-adding the tab while navigating between tabs.
			for (int i = 0; i < 3; i++)
			{
				App.Tap(tab1Title);
				App.WaitForElement(toggleTabButtonId);
				App.Tap(toggleTabButtonId); // Removes the tab
				App.Tap(toggleTabButtonId); // Re-adds the tab
				App.Tap(tab2Title);
				App.WaitForElement("label");
			}
		}
	}
}