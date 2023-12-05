using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollViewDeviceOrientationUITests : ScrollViewUITests
	{
		public ScrollViewDeviceOrientationUITests(TestDevice device)
			: base(device)
		{
		}

		// Issue773TestsRotationRelayoutIssue(src\Compatibility\ControlGallery\src\Issues.Shared\Issue773.cs)
		[Test]
		[Description("StackLayout respects device rotation.")]
		public void ScrollViewDeviceOrientationTest()
		{
			App.Click("ScrollViewDeviceOrientation");
			App.WaitForElement("WaitForStubControl");

			App.SetOrientationLandscape();

			var buttonLabels = new[] 
			{
				"Button 1",
				"Button 2",
				"Button 3",
			};

			foreach (string buttonLabel in buttonLabels)
				App.WaitForNoElement(buttonLabel);

			App.Screenshot("StackLayout respects rotation");

			App.SetOrientationPortrait();
		}
	}
}
