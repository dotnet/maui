using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33304 : _IssuesUITest
	{
		public Issue33304(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Shell TitleView disappears when switching tabs on Android";

		[Test]
		[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
		public void TitleViewPersistsAfterSwitchingTabs()
		{
			App.WaitForElement("HomeTabLabel");
			App.Tap("Search");
			App.WaitForElement("SearchTabLabel");
			App.Tap("Home");
			App.WaitForElement("HomeTabLabel");
			App.WaitForElement("HomeTitleView");
		}
	}
}
