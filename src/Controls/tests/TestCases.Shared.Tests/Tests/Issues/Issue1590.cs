using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1590 : _IssuesUITest
	{
		public Issue1590(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView.IsGroupingEnabled results ins ArguementOutOfRangeException";

		[Fact]
		[Trait("Category", UITestCategories.ListView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void ListViewIsGroupingEnabledDoesNotCrash()
		{
			App.WaitForElement("First");
		}
	}
}