#if IOS // Navigation from the more page is iOS specific
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27799 : _IssuesUITest
	{
		public Issue27799(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] OnAppearing and OnNavigatedTo does not work when using extended Tabbar";

[Test]
[Category(UITestCategories.Shell)]
public void OnAppearingAndOnNavigatedToShouldBeCalled()
		{
			App.WaitForElement("More");
			App.Click("More");
			App.WaitForElement("Tab6");
			App.Click("Tab6");
			App.WaitForElement("GoToSubpage6Button");
			App.Click("GoToSubpage6Button");
			App.Click("tab2");
			App.WaitForElement("OnNavigatedToCountLabel");
			var onNavigatedToCountLabel = App.FindElement("OnNavigatedToCountLabel").GetText();
			var onAppearingCountLabel = App.FindElement("OnAppearingCountLabel").GetText();

			ClassicAssert.AreEqual("OnNavigatedTo: 3", onNavigatedToCountLabel);
			ClassicAssert.AreEqual("OnAppearing: 3", onAppearingCountLabel);
		}
	}
}
#endif