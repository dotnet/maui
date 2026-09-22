using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38730 : _IssuesUITest
{
	public Issue38730(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Vertical Item Spacing page rendering issue on Mac 27";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyVerticalItemSpacingPageRendering()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");
		VerifyScreenshot();
	}
}
