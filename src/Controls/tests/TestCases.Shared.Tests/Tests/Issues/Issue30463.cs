using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30463 : _IssuesUITest
{
	public Issue30463(TestDevice device) : base(device) { }

	public override string Issue => "Picker title is not displayed again";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerShouldRegainTitle()
	{
		App.WaitForElement("ToggleSelectedIndexBtn");
		App.Tap("ToggleSelectedIndexBtn");
		VerifyScreenshot();
	}
}