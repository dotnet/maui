using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32984 : _IssuesUITest
{
	public Issue32984(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Picker on resize not working on Windows";

	[Test]
	[Category(UITestCategories.Picker)]
	public void Issue32984PickerShouldResize()
	{
		App.WaitForElement("issue32984Button");
		App.Tap("issue32984Picker");
#if ANDROID
		App.WaitForElement("Cancel");
		App.Tap("Cancel");
#elif IOS || MACCATALYST
		App.WaitForElement("Done");
		App.Tap("Done");
#elif WINDOWS
		App.TapCoordinates(10, 10); 
#endif
		App.Tap("issue32984Button");
		App.Tap("issue32984Picker");
		VerifyScreenshot();
	}
}