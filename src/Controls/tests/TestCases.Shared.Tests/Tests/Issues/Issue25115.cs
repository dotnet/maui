#if WINDOWS // Chevron icon color is only applicable to Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25115 : _IssuesUITest
{
	public override string Issue => "[Windows] The color was not applied properly to the Tab Chevron icon";

	public Issue25115(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyTabChevronIconColor()
	{
		App.WaitForElement("label");

		VerifyScreenshot();
	}
}
#endif