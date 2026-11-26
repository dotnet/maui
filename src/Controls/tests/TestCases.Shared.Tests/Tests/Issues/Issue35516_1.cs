using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35516_1 : _IssuesUITest
{
	public Issue35516_1(TestDevice device) : base(device)
	{
	}

	public override string Issue => "SearchHandler CharacterSpacing should apply initially";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void CharacterSpacingShouldApplyInitially()
	{
		App.WaitForElement("CharacterSpacingInstructions");
		VerifyScreenshot("InitialCharacterSpacing");
	}
}
