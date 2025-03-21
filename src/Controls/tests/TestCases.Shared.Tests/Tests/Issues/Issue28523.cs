using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28523 : _IssuesUITest
	{
		const string ButtonId = "SpinButton";
		const string Success = "Success";

		public Issue28523(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Different behavior on iOS and Android when Loop = False";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void AnimationCancel()
		{
			App.WaitForElement("Baboon");			
			App.SetOrientationLandscape();
			App.WaitForElement("Baboon");
			VerifyScreenshot();
		}
	}
}