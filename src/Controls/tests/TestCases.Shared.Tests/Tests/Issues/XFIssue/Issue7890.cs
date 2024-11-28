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

	//[Test]
	//[Category(UITestCategories.ListView)]
	//public void TestCorrectListItemsRemoved()
	//{
	//	App.WaitForElement(q => q.Button("RemoveBtn"));
	//	App.Tap(q => q.Button("RemoveBtn"));
	//	var toRemove = Enumerable.Range(RemoveFrom, RemoveCount).ToList();
	//	foreach (var c in Enumerable.Range(0, Count))
	//	{
	//		if (toRemove.Contains(c))
	//			Assert.IsNull(App.Query(q => q.Marked(c.ToString())).FirstOrDefault());
	//		else
	//			Assert.IsNotNull(App.Query(q => q.Marked(c.ToString())).FirstOrDefault());
	//	}
	//}
}