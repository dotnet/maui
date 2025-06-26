using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1469 : _IssuesUITest
	{
		const string Go = "Select 3rd item";
		const string Success = "Success";

		public Issue1469(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting SelectedItem to null inside ItemSelected event handler does not work";

		[Fact]
		[Trait("Category", UITestCategories.LifeCycle)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue1469Test()
		{
			App.WaitForElement(Go);
			App.Tap(Go);
			App.WaitForElement(Success);
		}
	}
}