using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue26846 : _IssuesUITest
{
	public Issue26846(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell PopToRootAsync doesn't happen instantly - previous pages flash quickly";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ModalPoppingShouldWorkOneByOne()
	{
		App.WaitForElement("OpenModalPage2");
		App.Click("OpenModalPage2");
		App.WaitForElement("OpenModalPage3");
		App.Click("OpenModalPage3");
		App.WaitForElement("CloseModalPage3");
		App.Click("CloseModalPage3");
		App.WaitForElement("CloseModalPage2");
		App.Click("CloseModalPage2");

		App.WaitForElement("OpenModalPage2");
	}
}