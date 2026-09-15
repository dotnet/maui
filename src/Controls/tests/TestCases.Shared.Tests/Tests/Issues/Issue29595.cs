#if IOSUITEST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29595 : _IssuesUITest
{
	public override string Issue => "iOS CV: GridItemsLayout not left-aligning a single item";

	public Issue29595(TestDevice device)
	: base(device)
	{ }

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyGridItemsLayoutLeftAlignsSingleItem()
	{
		App.WaitForElement("StubLabel");
		VerifyScreenshot();
	}
}
#endif