using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30752 : _IssuesUITest
{
	public Issue30752(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Default MinWidth in WinUI RadioButton interferes with custom ControlTemplates";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void VerifyRadioButtonIgnoresDefaultMinWidth()
	{
		App.WaitForElement("Issue30752RadioButton");
		VerifyScreenshot();
	}
}