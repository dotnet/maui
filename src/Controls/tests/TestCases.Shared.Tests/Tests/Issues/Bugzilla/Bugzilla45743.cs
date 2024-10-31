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

	// TODO From Xamarin.UITest Migration: needs better way to detect actionsheet
	// [Test]
	// [Category(UITestCategories.DisplayAlert)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void Bugzilla45743Test()
	// {
	// 	App.WaitForElement("ActionSheet Title");
	// 	App.Tap("Close");
	// 	App.WaitForElement("Title 2");
	// 	App.Tap("Accept");
	// 	App.WaitForElement("Title");
	// 	App.Tap("Accept");
	// 	Assert.That(App.FindElements("Page 2").Count, Is.GreaterThan(0));
	// }
}