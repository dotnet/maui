#if TEST_FAILS_ON_CATALYST  // Issue Link - https://github.com/dotnet/maui/issues/30163
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30065 : _IssuesUITest
{
	public Issue30065(TestDevice device) : base(device)
	{
	}

	public override string Issue => "DatePicker Ignores FlowDirection When Set to RightToLeft or MatchParent";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void ValidateDatePickerFlowDirection()
	{
		App.WaitForElement("ToggleFlowDirectionBtn");
		App.Tap("ToggleFlowDirectionBtn");
		VerifyScreenshot();
	}
}
#endif