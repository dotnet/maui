#if TEST_FAILS_ON_WINDOWS //On the Windows platform, when the "Remove" button is clicked, it only removes one item. Issue: https://github.com/dotnet/maui/issues/26377
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7890 : _IssuesUITest
{
	public Issue7890(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TemplatedItemsList incorrect grouped collection range removal";

	[Test]
	[Category(UITestCategories.ListView)]
	public void TestCorrectListItemsRemoved()
	{
		App.WaitForElement("RemoveBtn");
		App.Tap("RemoveBtn");
		var toRemove = Enumerable.Range(1, 5).ToList();
		foreach (var c in Enumerable.Range(0, 10))
		{
			if (toRemove.Contains(c))
				App.WaitForNoElement(c.ToString());
			else
				App.WaitForElement(c.ToString());
		}
	}
}
#endif