#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25192 : _IssuesUITest
	{
		public Issue25192(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CarouselView Loop='False' renders incorrectly items";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewShouldRenderCorrectly()
		{
			App.WaitForElement("Item1");
			VerifyScreenshot();
		}
	}
}
#endif