using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31096 : _IssuesUITest
{
	public Issue31096(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Changing IsGrouped on runtime with CollectionViewHandler2 does not properly work";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ChangingIsGroupedOnRuntime()
	{
		App.WaitForElement("Switch", timeout: TimeSpan.FromSeconds(15));
		App.Tap("Switch");
		App.WaitForElement("American Black Bear");
		App.WaitForElement("Asian Black Bear");

		App.Tap("Switch");
		App.WaitForNoElement("American Black Bear");
		App.WaitForNoElement("Asian Black Bear");

		App.Tap("Switch");
		App.WaitForElement("GroupHeader");
		App.WaitForElement("GroupFooter");
	}
}