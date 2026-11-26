using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29153 : _IssuesUITest
{
	public Issue29153(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Grouped CollectionView keeps the last data item in view";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void KeepLastItemInViewWithGroupedSource()
	{
		App.WaitForElement("First 0");
		App.Tap("AppendItem");
		App.WaitForElement("Last 30");
		App.Tap("AppendItem");
		App.WaitForElement("Last 31");
		App.Tap("ClearGroups");
		App.WaitForNoElement("Last 31");
		App.WaitForElement("AppendItem");
	}
}
