using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32723 : _IssuesUITest
{
	public Issue32723(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Inconsistent appearance of the Title property in Picker control on Windows";

	[Test]
	[Category(UITestCategories.Picker)]
	public void Issue32723PickerTitleDisplaysAsPlaceholder()
	{
		App.WaitForElement("issue32723Picker");
		VerifyPickerScreenshot();
	}

	[Test]
	[Category(UITestCategories.Picker)]
	public void Issue32723PickerTitleUpdatesAtRuntime()
	{
		App.WaitForElement("issue32723Button");
		App.Tap("issue32723Button");
		App.WaitForElement("issue32723Picker");
		VerifyPickerScreenshot();
	}

	void VerifyPickerScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif
	}
}
