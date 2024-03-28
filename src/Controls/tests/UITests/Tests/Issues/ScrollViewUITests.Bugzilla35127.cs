using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla35127UITests : _IssuesUITest
	{
		public Bugzilla35127UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "It is possible to craft a page such that it will never display on Windows";

		// Bugzilla35127 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla35127.cs)
		[Test]
		public void Issue35127Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac },	
				"This test is failing, likely due to product issue");

			App.WaitForNoElement("See me?");
			var count = App.FindElements("scrollView").Count;
			Assert.IsTrue(count == 0);
			App.WaitForNoElement("Click Me?");
		}
	}
}