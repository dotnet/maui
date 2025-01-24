/*
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

	[FailsOnIOSWhenRunningOnXamarinUITest]
	[FailsOnAndroidWhenRunningOnXamarinUITest]
	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void Bugzilla28001Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement("Push");

		App.Screenshot("I am at Bugzilla 28001");
		App.Tap("Push");

		string tab2String = "tab2";
#if ANDROID
		tab2String = tab2String.ToUpperInvariant();
#endif
		var tab2 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + tab2String + "']"));
		tab2.Click();

		string tab1String = "tab1";
#if ANDROID
		tab1String = tab1String.ToUpperInvariant();
#endif

		var tab1 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + tab1String + "']"));
		tab1.Click();

		App.Tap("Pop");

		Assert.That(App.FindElement("lblDisposedCount").GetText(),
			Is.EqualTo("Dispose 2 pages"));
	}
}
*/
