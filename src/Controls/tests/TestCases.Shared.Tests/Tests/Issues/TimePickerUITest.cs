#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // TimePicker not opens the dialog, issue: https://github.com/dotnet/maui/issues/10827 
// Picker popup layout is inconsistent on IOS , issue: https://github.com/dotnet/maui/issues/27898
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class TimePickerUITest : _IssuesUITest
	{
		public override string Issue => "TimePicker UI Test";

		public TimePickerUITest(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.TimePicker)]
		public void VerifyTimePickerApperance()
		{
			App.WaitForElement("TimePicker");
			App.Tap("TimePicker");
			VerifyScreenshot();
		}
	}
}
#endif
