using System.Diagnostics.Metrics;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18420 : _IssuesUITest
	{
		public Issue18420(TestDevice device) : base(device) { }

		public override string Issue => "ViewExtensions RotateYTo and RotateXTo with length 0 crashes on Windows";

		[Test]
		public void RotateYToNoCrashTest()
		{
			// Wait the Counter button
			App.WaitForElement("CounterBtn");

			// Tapping the Counter button will update the counter and also animate RotateY
			App.Click("CounterBtn");

			// If we can successfully tap the button (without a crash), the Counter label will be displayed
			Assert.IsTrue(App.WaitForTextToBePresentInElement("CounterLbl", "Clicked 1 time"));
		}
	}
}