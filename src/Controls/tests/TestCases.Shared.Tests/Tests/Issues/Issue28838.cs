using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28838 : _IssuesUITest
{
	public Issue28838(TestDevice device) : base(device) { }

	public override string Issue => "Incorrect Text Color Applied to Selected Tab in TabbedPage on Android";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void DefaultSelectedTabTextColorShouldApplyProperly()
	{
		App.WaitForElement("Tab1");
		VerifyScreenshot();
	}
}