#if WINDOWS
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
		VerifyScreenshot(cropTop: 100);
	}

	[Test]
	[Category(UITestCategories.Picker)]
	public void Issue32723PickerTitleUpdatesAtRuntime()
	{
		App.WaitForElement("issue32723Button");
		App.Tap("issue32723Button");
		App.WaitForElement("issue32723Picker");
		VerifyScreenshot(cropTop: 100);
	}
}
#endif
