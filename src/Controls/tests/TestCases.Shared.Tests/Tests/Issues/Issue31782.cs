#if TEST_FAILS_ON_WINDOWS //Issue - https://github.com/dotnet/maui/issues/4715. Appium does not find the AutomationID for the layout on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31782 : _IssuesUITest
{
	public Issue31782(TestDevice device) : base(device) { }

	public override string Issue => "Unexpected Line Breaks in Android, Label with WordWrap Mode Due to Trailing Space";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelWithEndAlignmentShouldNotHaveTrailingSpace()
	{
		var labelRect = App.WaitForElement("TestLabel").GetRect();
		var containerRect = App.WaitForElement("Container").GetRect();
		
		// Before fix: Label width ≈ container width (300pt with trailing space)
		// After fix: Label width < container width (measures only longest line)
		Assert.That(labelRect.Width, Is.LessThan(containerRect.Width - 10),
			$"End-aligned label ({labelRect.Width}pt) should be narrower than container ({containerRect.Width}pt)");
	}
}
#endif