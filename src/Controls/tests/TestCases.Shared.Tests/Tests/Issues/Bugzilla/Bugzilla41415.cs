#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
// On Android ScrollY and ScrollX values are resetted, Issue: https://github.com/dotnet/maui/issues/26747
// On Windows tests are failing in CI, but not locally. Need to investigate more.
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

		[Test]
		public void Bugzilla41415Test()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			App.WaitForElement(ButtonId);
			App.WaitForElementTillPageNavigationSettled("x: 100");
			App.WaitForElementTillPageNavigationSettled("y: 100");
			App.WaitForElement("z: True");
			App.WaitForElement("a: True");
			App.Tap(ButtonId);
			App.WaitForElement(ButtonId);
			App.WaitForElementTillPageNavigationSettled("y: 100");
			App.WaitForElement("z: True");
			App.WaitForElement("a: False");
			App.WaitForElementTillPageNavigationSettled("x: 200");
		}
	}
}
#endif
