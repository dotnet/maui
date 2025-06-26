using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2617 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue2617(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Error on binding ListView with duplicated items";

		[Fact]
		[Trait("Category", UITestCategories.ListView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void BindingToValuesTypesAndScrollingNoCrash()
		{
			Thread.Sleep(5000);
			App.WaitForElement(Success);
		}
	}
}