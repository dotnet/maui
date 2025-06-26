#if TEST_FAILS_ON_CATALYST // On Catalyst, the CheckBox was not able to be tapped in the CI.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CheckBoxUITest : _IssuesUITest
	{
		public override string Issue => "CheckBox UI Test";

		public CheckBoxUITest(TestDevice device)
		: base(device)
		{ }

		[Fact]
		[Trait("Category", UITestCategories.CheckBox), Order(1)]
		public void VerifyCheckBoxUnCheckedState()
		{
			App.WaitForElement("CheckBox");
			App.Tap("CheckBox");
			VerifyScreenshot();
		}

		[Fact]
		[Trait("Category", UITestCategories.CheckBox), Order(2)]
		public void VerifyCheckBoxCheckedState()
		{
			App.WaitForElement("CheckBox");
			App.Tap("CheckBox");
			VerifyScreenshot();
		}
	}
}
#endif