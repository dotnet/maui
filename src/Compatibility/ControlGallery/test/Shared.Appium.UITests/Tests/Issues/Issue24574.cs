using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue24574 : IssuesUITest
	{
		public Issue24574(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Tap Double Tap";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void TapThenDoubleTap()
		{
			RunningApp.Screenshot("I am at Issue 24574");

			RunningApp.WaitForElement("TapLabel");

			RunningApp.Tap("TapLabel");
			RunningApp.WaitForNoElement("Single");

			RunningApp.DoubleClick("TapLabel");
			RunningApp.WaitForNoElement("Double");
		}
	}
}