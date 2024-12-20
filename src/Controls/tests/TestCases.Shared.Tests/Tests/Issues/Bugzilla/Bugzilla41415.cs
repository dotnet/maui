#if TEST_FAILS_ON_ANDROID // ScrollY and ScrollX values are resetted on Android, Issue: https://github.com/dotnet/maui/issues/26747
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
			App.WaitForElementTillPageNavigationSettled("x: 100");
			App.WaitForElementTillPageNavigationSettled("y: 100");
			App.WaitForElement("z: True", timeout: TimeSpan.FromSeconds(25));
			App.WaitForElement("a: True");
			App.Tap(ButtonId);	
			App.WaitForElementTillPageNavigationSettled("y: 100");
			App.WaitForElement("z: True", timeout: TimeSpan.FromSeconds(25));
			App.WaitForElement("a: False");
			App.WaitForElementTillPageNavigationSettled("x: 200");
		}
	}
}
#endif