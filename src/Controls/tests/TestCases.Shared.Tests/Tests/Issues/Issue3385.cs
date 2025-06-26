using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3385 : _IssuesUITest
	{
		public Issue3385(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Entry's TextChanged event is fired on Unfocus even when no text changed";

		[Fact]
		[Trait("Category", UITestCategories.Entry)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue3385Test()
		{
			App.WaitForElement("entry");
			App.Tap("entry");
			App.WaitForElement("click");
			App.Tap("click");
			App.WaitForNoElement("FAIL");
		}
	}
}