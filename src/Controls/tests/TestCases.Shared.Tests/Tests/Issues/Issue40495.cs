#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40495 : _IssuesUITest
{
	public Issue40495(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Secondary toolbar overflow button gets Shell.ForegroundColor while the primary items do not";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 2)]
	public void OverflowButtonMatchesPrimaryToolbarItemTint()
	{
		App.WaitForElement("Issue40495_DescriptionLabel");
		VerifyScreenshot();
	}
}
#endif
