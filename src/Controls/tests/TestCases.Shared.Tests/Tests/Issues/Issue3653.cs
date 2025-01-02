#if !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3653 : _IssuesUITest
{
	public Issue3653(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Loses the correct reference to the cell after adding and removing items to a ListView";

	// [Test]
	// [Category(UITestCategories.ContextActions)]
	// [FailsOnAndroid]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void TestRemovingContextMenuItems()
	// {
	// 	for (int i = 1; i <= 3; i++)
	// 	{
	// 		string searchFor = $"Remove me using the context menu. #{i}";
	// 		App.WaitForElement(searchFor);

	// 		App.ActivateContextMenu(searchFor);
	// 		App.WaitForElement(c => c.Marked("Remove"));
	// 		App.Tap(c => c.Marked("Remove"));
	// 	}

	// 	for (int i = 4; i <= 6; i++)
	// 	{
	// 		App.Tap("Add an item");
	// 		string searchFor = $"Remove me using the context menu. #{i}";

	// 		App.ActivateContextMenu(searchFor);
	// 		App.WaitForElement(c => c.Marked("Remove"));
	// 		App.Tap(c => c.Marked("Remove"));
	// 	}


	// 	for (int i = 1; i <= 6; i++)
	// 	{
	// 		string searchFor = $"Remove me using the context menu. #{i}";
	// 		App.WaitForNoElement(c => c.Marked("Remove"));
	// 	}
	// }
}
#endif