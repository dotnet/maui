using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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
			App.WaitForElement("See me?");
			var count = App.FindElements("scrollView").Count;
			ClassicAssert.IsTrue(count == 0);
			App.WaitForNoElement("Click Me?");
		}
	}
}