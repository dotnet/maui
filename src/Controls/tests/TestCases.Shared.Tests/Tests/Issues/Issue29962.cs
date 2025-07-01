using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29962 : _IssuesUITest
{
	public override string Issue => "[Windows] SearchBar PlaceHolder and Background Color should update properly at runtime";

	public Issue29962(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void VerifySearchBarPlaceholderAndBackgroundColor()
	{
		App.WaitForElement("ColorChangeButton");
		App.Tap("ColorChangeButton");
		VerifyScreenshot();
	}
}