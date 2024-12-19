#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST &&TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //y value not changing
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	[Category(UITestCategories.Compatibility)]
	public class Bugzilla41415UITests : _IssuesUITest
	{
		const string ButtonId = "ClickId";

		public Bugzilla41415UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollX and ScrollY values are not consistent with iOS";

		// Bugzilla41415 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla41415.cs)
		[Test]
		public void Bugzilla41415Test()
		{
			// This test is failing, likely due to product issue

			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			App.WaitForElement("x: 100");
			App.WaitForElement("y: 100");
			App.WaitForElement("z: True");
			App.WaitForElement("a: True");
			App.Tap(ButtonId);
			App.WaitForElement("x: 200");
			App.WaitForElement("y: 100");
			App.WaitForElement("z: True");
			App.WaitForElement("a: False");
		}
	}
}
#endif