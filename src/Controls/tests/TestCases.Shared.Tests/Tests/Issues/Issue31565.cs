using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31565 : _IssuesUITest
	{
		public Issue31565(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "FlexLayout alignment issue when Wrap is set to Reverse and AlignContent is set to SpaceAround, SpaceBetween or SpaceEvenly";

		[Test, Order(1)]
		[ShardedTestCategory(UITestCategories.Layout, shard: 3)]
		public void FlexLayoutReverseWithSpaceAround()
		{
			App.WaitForElement("SpaceAroundButton");
			App.Tap("SpaceAroundButton");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[ShardedTestCategory(UITestCategories.Layout, shard: 3)]
		public void FlexLayoutReverseWithSpaceBetween()
		{
			App.WaitForElement("SpaceBetweenButton");
			App.Tap("SpaceBetweenButton");
			VerifyScreenshot();
		}

		[Test, Order(3)]
		[ShardedTestCategory(UITestCategories.Layout, shard: 3)]
		public void FlexLayoutReverseWithSpaceEvenly()
		{
			App.WaitForElement("SpaceEvenlyButton");
			App.Tap("SpaceEvenlyButton");
			VerifyScreenshot();
		}
	}
}
