using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellSearchHandlerItemSizing : _IssuesUITest
{
	public ShellSearchHandlerItemSizing(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Search Handler Item Sizing";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void SearchHandlerSizesCorrectly()
	//{
	//	RunningApp.WaitForElement("SearchHandler");
	//	RunningApp.EnterText("SearchHandler", "Hello");
	//	var contentSize = RunningApp.WaitForElement("searchresult")[0].Rect;
	//	Assert.Less(contentSize.Height, 100);
	//}
}