using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue38821 : _IssuesUITest
	{
		public Issue38821(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CurrentItem is incorrect after removing an item from the CarouselView (Loop = false)";

		[Test]
		[ShardedTestCategory(UITestCategories.CarouselView, shard: 1)]
		public void TestCurrentItemAfterRemoval()
		{
			App.WaitForElement("Item0");
			App.SwipeRightToLeft("Issue38821_CarouselView");
			App.Tap("Issue38821_Button");
			App.WaitForElement("Item2");
		}
	}
}
