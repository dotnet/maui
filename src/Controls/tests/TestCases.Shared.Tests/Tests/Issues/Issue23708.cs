using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23708 : _IssuesUITest
{
	public Issue23708(TestDevice device) : base(device) { }

	public override string Issue => "Grouped CollectionView has extra space before group headers";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void Issue23708GroupHeaderDoesNotHaveExtraVerticalItemSpacing()
	{
		App.WaitForElement("GroupedCollectionView");
		VerifyScreenshot();
	}
}
