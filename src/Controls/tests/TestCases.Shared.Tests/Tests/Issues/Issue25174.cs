using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25174 : _IssuesUITest
	{
		const string CapturePhotoAsync = "CapturePhotoAsync";
		const string Allow = "Allow";
		

		public Issue25174(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Camera preview is freezing when rotating and using FlyoutPage on iOS #25174";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void ScrollViewInHeaderDisposesProperly()
		{
			App.SetOrientationLandscape();
			App.WaitForElement(CapturePhotoAsync);
			App.Tap(CapturePhotoAsync);
			App.WaitForElement(Allow);
			App.Tap(Allow);
			App.SetOrientationPortrait();

			//TODO tap X button in camera screen
			

			App.WaitForElement(CapturePhotoAsync);
		}
	}
}