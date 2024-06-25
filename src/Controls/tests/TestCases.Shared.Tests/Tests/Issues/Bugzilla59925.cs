#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla59925 : _IssuesUITest
	{
		public Bugzilla59925(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Font size does not change vertical height of Entry on iOS";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void Issue123456Test()
		{
			App.Screenshot("I am at Issue 59925");
			App.WaitForElement("Bigger");
			App.Screenshot("0");

			App.Tap("Bigger");
			App.Screenshot("1");

			App.Tap("Bigger");
			App.Screenshot("2");

			App.Tap("Bigger");
			App.Screenshot("3");
		}
	}
}
#endif