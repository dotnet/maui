#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1658 : _IssuesUITest
{
	public Issue1658(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS] GestureRecognizer on ListView Item not working";
	
	// [Test]
	// [Category(UITestCategories.ListView)]
	// public void ContextActionsIconImageSource()
	// {
	// 	RunningApp.ActivateContextMenu("ListViewItem");
	// 	RunningApp.WaitForElement("coffee.png");
	// 	RunningApp.DismissContextMenu();

	// 	RunningApp.WaitForElement("ColorBox");
	// 	RunningApp.Screenshot("Box should be red");
	// 	RunningApp.Tap("ColorBox");
	// 	RunningApp.Screenshot("Box should be yellow");
	// }
}
#endif