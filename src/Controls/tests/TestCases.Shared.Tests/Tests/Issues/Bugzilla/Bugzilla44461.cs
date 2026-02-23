using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla44461UITests : _IssuesUITest
	{
		public Bugzilla44461UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollToPosition.Center works differently on Android and iOS";

		[Test]
		public void Bugzilla44461Test()
		{
			App.Tap("Scroll to Button0");
			VerifyScreenshot();
		}
	}
}