#if WINDOWS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3652 : _IssuesUITest
{
	public Issue3652(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Loses the correct reference to the cell after adding and removing items to a ListView";

	[Test]
	[Category(UITestCategories.ContextActions)]
	public void TestRemovingContextMenuItems()
	{
		for (int i = 1; i <= 3; i++)
		{
			string searchFor = $"Remove me using the context menu. #{i}";
			App.WaitForElement(searchFor);

			App.ContextActions(searchFor);
			App.WaitForElement("Remove");
			App.Tap("Remove");
		}

		for (int i = 4; i <= 6; i++)
		{
			App.Tap("Add an item");
			string searchFor = $"Remove me using the context menu. #{i}";

			App.ContextActions(searchFor);
			App.WaitForElement("Remove");
			App.Tap("Remove");
		}


		for (int i = 1; i <= 6; i++)
		{
			string searchFor = $"Remove me using the context menu. #{i}";
			App.WaitForNoElement(searchFor);
		}
	}
}
#endif