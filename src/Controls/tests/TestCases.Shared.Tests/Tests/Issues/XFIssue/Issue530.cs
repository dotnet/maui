#if TEST_FAILS_ON_CATALYST // Getting OpenQA.Selenium.InvalidSelectorException on Catalyst Line No. 23.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue530 : _IssuesUITest
{
	public Issue530(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView does not render if source is async";

	[Fact]
	[Trait("Category", UITestCategories.ListView)]
	public void Issue530TestsLoadAsync()
	{
		App.WaitForElement("Load");
		App.Tap("Load");

		App.WaitForElement("John");
		App.FindElement("Paul");
		App.FindElement("George");
		App.FindElement("Ringo");
	}
}
#endif