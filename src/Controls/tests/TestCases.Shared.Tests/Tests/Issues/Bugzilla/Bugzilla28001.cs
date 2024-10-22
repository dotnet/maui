using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla28001 : _IssuesUITest
{
    public Bugzilla28001(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "[Android] TabbedPage: invisible tabs are not Disposed";

	// [FailsOnIOS]
	// [FailsOnAndroid]
	// [Test]
	// [Category(UITestCategories.TabbedPage)]		
	// public void Bugzilla28001Test()
	// {
	// 	App.WaitForElement("Push");

	// 	App.Screenshot("I am at Bugzilla 28001");
	// 	App.Tap("Push");
	// 	App.Tap("Tab2");
	// 	App.Tap("Tab1");
	// 	App.Tap("Pop");

	// 	Assert.That(App.FindElement("lblDisposedCount").GetText(),
	// 		Is.EqualTo("Dispose 2 pages"));
	// }
}