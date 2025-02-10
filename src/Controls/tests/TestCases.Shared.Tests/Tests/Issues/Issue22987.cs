#if WINDOWS
// In MacCatalyst, DatePicker Text color property is not working MacOS https://github.com/dotnet/maui/issues/20904
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22987 : _IssuesUITest
{
	public Issue22987(TestDevice device) : base(device) { }

	public override string Issue => "DatePicker Color Icon";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerTextandIconColorShouldWorkProperlyOnPointerOver()
	{
		App.WaitForElement("Label");

		//need to include something like a MovePointer and MovePointerToCoordinate methods for hovering.

		VerifyScreenshot();
	}
}
#endif