#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32722 : _IssuesUITest
	{
		public override string Issue => "NavigationPage.TitleView does not expand with host window in iPadOS 26+";
		public Issue32722(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Navigation)]
		public void TitleViewExpandsOnRotation()
		{
			// Wait for page to load
			App.WaitForElement("TitleViewGrid");
			App.WaitForElement("StatusLabel");

			// Get initial orientation and TitleView bounds
			var titleViewInitial = App.WaitForElement("TitleViewGrid").GetRect();
			var initialWidth = titleViewInitial.Width;
			
			App.SetOrientationLandscape();

			// Wait for rotation to complete
			System.Threading.Thread.Sleep(2000);

			// Get TitleView bounds after rotation
			var titleViewAfterRotation = App.WaitForElement("TitleViewGrid").GetRect();
			var newWidth = titleViewAfterRotation.Width;

			// On iOS 26+, the TitleView should expand/contract with the rotation
			// The bug was that it would stay at the original width
			// After fix, the width should change to match the new navigation bar width
			Assert.That(newWidth, Is.Not.EqualTo(initialWidth).Within(100), 
				"TitleView width should change after rotation");

			// Verify TitleView is still visible and has reasonable dimensions
			Assert.That(newWidth, Is.GreaterThan(100), 
				"TitleView should have a reasonable width after rotation");

			// Rotate back to original orientation
			App.SetOrientationPortrait();
			System.Threading.Thread.Sleep(2000);

			// Verify TitleView returns to approximately original width
			var titleViewFinal = App.WaitForElement("TitleViewGrid").GetRect();
			Assert.That(titleViewFinal.Width, Is.EqualTo(initialWidth).Within(5),
				"TitleView should return to original width after rotating back");
		}
	}
}
#endif