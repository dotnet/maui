#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25201 : _IssuesUITest
	{
		public Issue25201(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "[Android] ImageButton Padding Incorrect After IsVisible False";

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void ImageButtonPaddingDoesNotChangeWhenIsVisibleChanges()
		{
			App.WaitForElement("Switch1");

			// https://github.com/dotnet/maui/issues/25201
			App.Tap("Switch1"); // ImageButton IsVisible changes to true
								// https://github.com/dotnet/maui/issues/16713
			App.Tap("Switch2"); // Hides overlay ContentView

			VerifyScreenshot();

			// https://github.com/dotnet/maui/issues/18001
			App.Tap("Switch1"); // ImageButton IsVisible changes to false
			App.Tap("Switch1"); // ImageButton IsVisible changes to true

			VerifyScreenshot();
		}
	}
}
#endif
