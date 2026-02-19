using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32266 : _IssuesUITest
{
	public Issue32266(TestDevice device) : base(device)
	{
	}

	public override string Issue => "TimePicker on Windows Defaults to Midnight When Time Value Is Null";

	[Test, Order(1)]
	[Category(UITestCategories.TimePicker)]
	public void VerifyTimePickerIsNullOnInitialLoad()
	{
		App.WaitForElement("Issue32266TimePicker");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.TimePicker)]
	public void VerifyClearedTimeDoesNotShowMidnight()
	{
		App.WaitForElement("SetTimeButton");
		App.Tap("SetTimeButton");

		App.WaitForElement("ClearTimeButton");
		App.Tap("ClearTimeButton");

		VerifyScreenshot();
	}
}