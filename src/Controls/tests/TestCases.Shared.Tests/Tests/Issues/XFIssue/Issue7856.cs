using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7856 : _IssuesUITest
{
	public Issue7856(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug]  Shell BackButtonBehaviour TextOverride breaks back";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void BackButtonBehaviorTest()
	//{
	//	App.Tap(x => x.Text("Tap to Navigate To the Page With BackButtonBehavior"));

	//	App.WaitForElement(x => x.Text("Navigate again"));

	//	App.Tap(x => x.Text("Navigate again"));

	//	App.WaitForElement(x => x.Text("Test"));

	//	App.Tap(x => x.Text("Test"));
	//}
}