using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla57758 : _IssuesUITest
	{
		const string ImageId = "TestImageId";

		public Bugzilla57758(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ObjectDisposedException for Microsoft.Maui.Controls.Platform.Android.FastRenderers.ImageRenderer";

		[Test]
		[Category(UITestCategories.Image)]
		public void RemovingImageWithGestureFromLayoutWithinGestureHandlerDoesNotCrash()
		{
			App.WaitForElement(ImageId);
			App.Tap(ImageId);
		}
	}
}