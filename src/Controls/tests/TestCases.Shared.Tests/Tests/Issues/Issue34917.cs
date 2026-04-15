using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34917 : _IssuesUITest
	{
		public Issue34917(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SwipeView.Open crashes with ArgumentException on second call";

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void SwipeViewOpenDoesNotCrashOnSecondCall()
		{
			App.WaitForElement("OpenButton");

			// First open
			App.Tap("OpenButton");
			App.WaitForElement("StatusLabel");
			var status1 = App.FindElement("StatusLabel").GetText();
			Assert.That(status1, Is.EqualTo("Opened 1"), "First Open should succeed");

			// Close
			App.Tap("CloseButton");

			// Wait for close animation
			Task.Delay(500).Wait();

			// Second open - this used to crash with ArgumentException
			App.Tap("OpenButton");
			var status2 = App.FindElement("StatusLabel").GetText();
			Assert.That(status2, Is.EqualTo("Opened 2"), "Second Open should succeed without crash");
		}
	}
}
