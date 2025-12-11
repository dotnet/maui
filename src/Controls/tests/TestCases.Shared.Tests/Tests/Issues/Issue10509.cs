using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10509 : _IssuesUITest
{
	public Issue10509(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Query parameter is missing after navigation";

	[Test]
	[Category(UITestCategories.Shell)]
	public void QueryIsPassedOnNavigation()
	{
		App.WaitForElement("Page1Button");
		App.Tap("Page1Button");
		var label = App.WaitForElement("Page2Label");
		Assert.That(label.GetText(), Is.EqualTo("Navigation data: Passed"));
	}
}