using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla43469 : _IssuesUITest
{

	public Bugzilla43469(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Calling DisplayAlert twice in WinRT causes a crash";

	// TODO From Xamarin.UITest Migration: test fails. Maybe we need to wait on the alert?
	// [Test]
	// [Category(UITestCategories.DisplayAlert)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public async Task Bugzilla43469Test()
	// {
	// 	App.WaitForElement("kButton");
	// 	App.Tap("kButton");
	// 	Assert.That(App.GetAlert()?.GetAlertText(), Is.EqualTo("First"));
	// 	App.GetAlert()?.DismissAlert();
	// 	Assert.That(App.GetAlert()?.GetAlertText(), Is.EqualTo("Second"));
	// 	App.GetAlert()?.DismissAlert();
	// 	Assert.That(App.GetAlert()?.GetAlertText(), Is.EqualTo("Three"));
	// 	App.GetAlert()?.DismissAlert();

	// 	await Task.Delay(100);
	// 	App.GetAlert()?.DismissAlert();
	// 	await Task.Delay(100);
	// 	App.GetAlert()?.DismissAlert();
	// 	await Task.Delay(100);
	// 	App.GetAlert()?.DismissAlert();
	// 	await Task.Delay(100);
	// 	App.WaitForElement("kButton");
	// }
}