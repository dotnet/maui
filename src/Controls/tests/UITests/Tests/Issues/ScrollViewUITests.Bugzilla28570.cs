using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla28570UITests : _IssuesUITest
	{
		public Bugzilla28570UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=28570";

		// Bugzilla28570 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla28570.cs)
		[Test]
		public void Bugzilla28570Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android },
				"This test is failing, likely due to product issue");

			App.WaitForElement ("Tap");
			App.Screenshot ("At test page");
			App.Click ("Tap");

			App.WaitForElement ("28570Target");
		}
	}
}