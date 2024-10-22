using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57317 : _IssuesUITest
{
	const string Success = "Success";
	const string BtnAdd = "btnAdd";

	public Bugzilla57317(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Modifying Cell.ContextActions can crash on Android";

	// [Test]
	// [Category(UITestCategories.TableView)]
	// [FailsOnIOS]
	// public void Bugzilla57317Test()
	// {
	// 	RunningApp.WaitForFirstElement("Cell");

	// 	RunningApp.ActivateContextMenu("Cell");

	// 	RunningApp.WaitForFirstElement("Self-Deleting item");
	// 	RunningApp.Tap(c => c.Marked("Self-Deleting item"));
	// }
}