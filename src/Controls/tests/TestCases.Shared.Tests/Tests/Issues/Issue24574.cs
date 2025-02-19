using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24574 : _IssuesUITest
	{
		public Issue24574(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Tap Double Tap";

		[Test]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Compatibility)]
		public void TapThenDoubleTap()
		{
			App.Screenshot("I am at Issue 24574");

			App.WaitForElement("TapLabel");

			App.Tap("TapLabel");
			App.WaitForNoElement("Single");

			App.DoubleTap("TapLabel");
			App.WaitForNoElement("Double");
		}
	}
}