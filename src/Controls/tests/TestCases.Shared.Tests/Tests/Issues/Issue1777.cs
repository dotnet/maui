#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1777 : _IssuesUITest
{
	public Issue1777(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Adding picker items when picker is in a ViewCell breaks";

	// [Test]
	// [Category(UITestCategories.TableView)]
	// public void Issue1777Test()
	// {
	// 	RunningApp.WaitForElement(q => q.Button(_btnText));
	// 	RunningApp.Tap(q => q.Button(_btnText));
	// 	RunningApp.Tap(q => q.Marked(_pickerTableId));
	// 	RunningApp.WaitForElement(q => q.Marked("test 0"));
	// 	RunningApp.Screenshot("Picker is displayed correctly in the ViewCell");
	// }
}
#endif