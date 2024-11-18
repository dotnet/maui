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
	//	App.WaitForElement(c => c.Marked("Email"));
	//	App.ActivateContextMenu("Subject Line 0");
	//	App.WaitForElement("Delete");
	//	App.Tap("Delete");
	//	App.ActivateContextMenu("Subject Line 1");
	//	App.Tap("Delete");
	//	App.WaitForElement(c => c.Marked(Success));
	//	App.Back();
	//	App.WaitForElement(c => c.Marked("Email"));
	//	App.SwipeRightToLeft();
	//}
}