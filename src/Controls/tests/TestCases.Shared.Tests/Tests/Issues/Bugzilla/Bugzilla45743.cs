#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST
// DisplayActionSheet and DisplayAlert are popped up in the constructor using BeginInvokeOnMainThread which is not working on Windows, Android, and Catalyst. Issue : https://github.com/dotnet/maui/issues/26481
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla45743 : _IssuesUITest
{
	public Bugzilla45743(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Calling DisplayAlert via BeginInvokeOnMainThread blocking other calls on iOS";

	[Test]
	[Category(UITestCategories.DisplayAlert)]
	public void Bugzilla45743Test()
	{
		App.WaitForElement("ActionSheet Title");
		App.Tap("Close");
		App.WaitForElement("Title 2");
		App.Tap("Accept");
		App.WaitForElement("Title");
		App.Tap("Accept");
		Assert.That(App.FindElements("Page 2").Count, Is.GreaterThan(0));
	}
}
#endif