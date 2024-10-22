using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4314 : _IssuesUITest
{
	public Issue4314(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "When ListView items is removed and it is empty, Xamarin Forms crash";

	// TODO: See HostApp UI class for this, some supporting types are missing from code there. We probably need to get those our of the ControlGallery project or replace them
	//[Test]
	//[Category(UITestCategories.ContextActions)]
	//public void Issue4341Test()
	//{
	//	RunningApp.WaitForElement(c => c.Marked("Email"));
	//	RunningApp.ActivateContextMenu("Subject Line 0");
	//	RunningApp.WaitForElement("Delete");
	//	RunningApp.Tap("Delete");
	//	RunningApp.ActivateContextMenu("Subject Line 1");
	//	RunningApp.Tap("Delete");
	//	RunningApp.WaitForElement(c => c.Marked(Success));
	//	RunningApp.Back();
	//	RunningApp.WaitForElement(c => c.Marked("Email"));
	//	RunningApp.SwipeRightToLeft();
	//}
}