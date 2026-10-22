using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39534 : _IssuesUITest
{
	public Issue39534(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView dates truncate after existing item properties change with MeasureFirstItem";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void DateLabelsShouldFitFullTextAfterExistingItemPropertiesAreSwapped()
	{
		App.WaitForElement("Issue39534SwapButton");

		for (int swapCount = 1; swapCount <= 6; swapCount++)
		{
			App.Tap("Issue39534SwapButton");
		}

		VerifyScreenshot();
	}
}