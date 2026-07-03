using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29729 : _IssuesUITest
{
	public override string Issue => "RadioButton TextTransform Property Does Not Apply on Android and Windows Platforms";

	public Issue29729(TestDevice device)
	: base(device)
	{ }

	[Test, Order(1)]
	[Category(UITestCategories.RadioButton)]
	public void VerifyRadioButtonTextWithLowerTransform()
	{
		App.WaitForElement("radioButton");
		App.Tap("LowerCaseButton");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.RadioButton)]
	public void VerifyRadioButtonTextWithUpperTransform()
	{
		App.WaitForElement("radioButton");
		App.Tap("UpperCaseButton");
		VerifyScreenshot();
	}
}