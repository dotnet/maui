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
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Compatibility)]
		public void TapThenDoubleTap()
		{
			App.Screenshot("I am at Issue 24574");

			App.WaitForElement("TapLabel");

			App.Tap("TapLabel");
			App.WaitForElement("Single");

			App.DoubleTap("TapLabel");

#if ANDROID || IOS // In CI Double tap does not effective sometimes so retry once before failing to resolve the flakiness.

			try
			{
				App.WaitForElement("Double");
			}
			catch (TimeoutException)
			{
				App.WaitForElement("Single");
				App.DoubleTap("TapLabel");
			}
#endif
			App.WaitForElement("Double");
		}
	}
}