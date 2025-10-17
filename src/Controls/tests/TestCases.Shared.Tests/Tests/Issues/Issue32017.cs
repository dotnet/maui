#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32017 : _IssuesUITest
	{
		public Issue32017(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Image shifts downward when window is resized smaller";

		[Test]
		[Category(UITestCategories.Image)]
		[Category(UITestCategories.Layout)]
		public void Issue32017Test()
		{
			App.WaitForElement("RecipeCarousel");
			App.WaitForElement("RecipeImage");
			App.WaitForElement("InstructionLabel");
			
			VerifyScreenshot("Issue32017ImageLayout");
			// Just verify the elements are present and positioned correctly
		}
	}
}
#endif