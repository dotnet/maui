using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32356 : _IssuesUITest
	{
		public override string Issue => "MauiImage with LogicalName containing path - is not working on Windows";

		public Issue32356(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Image)]
		public void ImageWithPathInLogicalNameShouldLoad()
		{
			// Wait for the image element to be present
			App.WaitForElement("TestImage");

			// Verify the image loaded successfully by checking it's visible
			// The test passes if the Image element is found and displayed
			// If the image failed to load due to the bug, the element would still exist but show broken image
			// Screenshot verification will catch visual issues
			VerifyScreenshot();
		}
	}
}
