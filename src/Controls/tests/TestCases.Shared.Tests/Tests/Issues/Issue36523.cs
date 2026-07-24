#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36523 : _IssuesUITest
{
	public Issue36523(TestDevice device) : base(device) { }

	public override string Issue => "MediaPicker.PickPhotosAsync hangs after device rotation on API 33+";

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}

	[Test]
	[Category(UITestCategories.Essentials)]
	public void PickPhotosAsyncShouldReturnAfterRotation()
	{
		// Bug only manifests on API 33+ where the native Photo Picker is used.
		if (App is AppiumApp appiumApp)
		{
			var apiLevel = (long?)appiumApp.Driver.Capabilities.GetCapability("deviceApiLevel") ?? 0;
			if (apiLevel < 33)
			{
				Assert.Ignore($"Issue #36523 only manifests on Android API 33+. Current device API: {apiLevel}.");
			}
		}

		App.WaitForElement("OpenRotationActivityButton");
		App.Tap("OpenRotationActivityButton");

		App.WaitForElement("RotationActivityPickButton");
		App.WaitForElement("RotationActivityStatusLabel");

		App.Tap("RotationActivityPickButton");
		Task.Delay(2000).Wait();

		// Rotate while picker is open — triggers activity destroy/recreate
		App.SetOrientationLandscape();
		Task.Delay(2000).Wait();

		// Cancel the picker
		App.Back();

		// With fix: task completes → "PASS". Without fix: task hangs → "FAIL".
		var returned = App.WaitForTextToBePresentInElement("RotationActivityStatusLabel", "PASS",
			timeout: TimeSpan.FromSeconds(30));

		var resultText = App.FindElement("RotationActivityStatusLabel").GetText();

		Assert.That(resultText, Does.Contain("PASS"),
			$"PickPhotosAsync must complete after device rotation. Actual: '{resultText}'.");

		Assert.That(resultText, Does.Not.Contain("FAIL"),
			$"Picker task hung after activity recreation: '{resultText}'.");

		App.Back();
		App.WaitForElement("OpenRotationActivityButton");
	}
}
#endif
