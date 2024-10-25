using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4303 : _IssuesUITest
{
	public Issue4303(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] TabbedPage's child is appearing before it should be";

	//[Test]
	//[Category(UITestCategories.LifeCycle)]
	//public void Issue4303Test()
	//{
	//	App.WaitForElement(c => c.Text(Success));
	//	App.WaitForElement(c => c.Marked(btnAutomationID));
	//	App.Tap(c => c.Marked(btnAutomationID));
	//	App.WaitForElement(c => c.Text(ChildSuccess));
	//}
}