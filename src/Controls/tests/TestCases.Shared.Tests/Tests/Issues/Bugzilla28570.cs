#if !ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	[Category(UITestCategories.Compatibility)]
	public class Bugzilla28570UITests : _IssuesUITest
	{
		public Bugzilla28570UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=28570";

		// Bugzilla28570 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla28570.cs)
		[Test]
		[FailsOnAndroid("This test is failing, likely due to product issue")]
		public void Bugzilla28570Test()
		{
			App.WaitForElement ("Tap");
			App.Screenshot ("At test page");
			App.Tap("Tap");

			App.WaitForElement ("28570Target");
		}
	}
}
#endif