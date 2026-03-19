#if IOS // Navigation from the more page is iOS specific
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27800 : _IssuesUITest
	{
		public Issue27800(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Shell.BackButtonBehavior does not work when using extended Tabbar";

[Test]
[Category(UITestCategories.Shell)]
public void ShellBackButtonBehaviorShouldWorkWithMoreTab()
		{
			App.WaitForElement("More");
			App.Click("More");
			App.WaitForElement("tab6");
			App.Click("tab6");
			App.WaitForElement("button");
			App.Click("button");
			App.WaitForElement("Go Back");
			App.Click("Go Back");
			App.WaitForElement("OnNavigatedToCountLabel");
			var onNavigatedToCountLabel = App.FindElement("OnNavigatedToCountLabel").GetText();
			var onAppearingCountLabel = App.FindElement("OnAppearingCountLabel").GetText();

			ClassicAssert.AreEqual("OnNavigatedTo: 2", onNavigatedToCountLabel);
			ClassicAssert.AreEqual("OnAppearing: 2", onAppearingCountLabel);
		}
	}
}
#endif