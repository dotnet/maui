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
	// 	App.WaitForElement(q => q.Button(_btnText));
	// 	App.Tap(q => q.Button(_btnText));
	// 	App.Tap(q => q.Marked(_pickerTableId));
	// 	App.WaitForElement(q => q.Marked("test 0"));
	// 	App.Screenshot("Picker is displayed correctly in the ViewCell");
	// }
}
#endif