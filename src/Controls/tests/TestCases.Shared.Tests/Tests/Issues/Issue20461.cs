using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20461 : _IssuesUITest
	{
		public Issue20461(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Nested FlexLayouts";

		[Test]
		[Category(UITestCategories.Layout)]
		public void NestedFlexLayoutShouldRenderCorrectly()
		{
			App.WaitForElement("imageInNestedLayout");
			App.WaitForElement("imageButtonInNestedLayout");
			VerifyScreenshot();
		}
	}
}
