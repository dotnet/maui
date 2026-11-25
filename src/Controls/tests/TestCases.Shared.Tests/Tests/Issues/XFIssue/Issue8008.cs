using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8008 : _IssuesUITest
{
	public Issue8008(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Removing Shell Item can cause Shell to try and set a MenuItem as the default visible item";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 2)]
	public void RemovingShellItemCorrectlyPicksNextValidShellItemAsVisibleShellItem()
	{
		App.WaitForElement("Success");
	}
}