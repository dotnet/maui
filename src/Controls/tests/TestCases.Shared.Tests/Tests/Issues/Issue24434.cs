using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24434 : _IssuesUITest
	{
		public Issue24434(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Modifying a layout while view isn't part of the Window fails to update the layout visually";

		[Fact]
		[Trait("Category", UITestCategories.Layout)]
		public void ModifyingANonVisibleLayoutWorks()
		{
			App.WaitForElement("ClickMe");
			App.Tap("ClickMe");
			App.WaitForElement("Success");
		}
	}
}