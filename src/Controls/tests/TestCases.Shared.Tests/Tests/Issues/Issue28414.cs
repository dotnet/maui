using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28414 : _IssuesUITest
{
	public Issue28414(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "NavigationStack updated when OnAppearing triggered";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NavigationStackUpdatesOnPop()
	{
		App.WaitForElement("FirstPageButton");
		App.Tap("FirstPageButton");
		App.WaitForElement("SecondPageButton");
		App.Tap("SecondPageButton");
		App.WaitForElement("ThirdPageButton");
		App.Tap("ThirdPageButton");
		var label = App.WaitForElement("OnAppearingLabel");
		Assert.That(label.GetText(), Is.EqualTo("Stack has 2 pages"));
	}
}