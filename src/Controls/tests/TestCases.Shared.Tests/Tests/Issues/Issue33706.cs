#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33706 : _IssuesUITest
	{
		public override string Issue => "MediaPicker gets stuck if LaunchMode is SingleTask";

		public Issue33706(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void Issue33706Test()
		{
			// Wait for the page to load
			App.WaitForElement("PickMediaButton");
			App.WaitForElement("StatusLabel");
			var initialStatus = App.FindElement("StatusLabel").GetText();
			Assert.That(initialStatus, Is.EqualTo("Ready"), "Initial status should be 'Ready'");
			App.Tap("PickMediaButton");
			Task.Delay(500).Wait();
			App.BackgroundApp();
			// Wait for app to background and resume
			Task.Delay(500).Wait();
			App.ForegroundApp();
			App.WaitForElement("StatusLabel");
			App.WaitForElement("PickMediaButton");
			var finalStatus = App.FindElement("StatusLabel").GetText();
			Assert.That(finalStatus, Is.Not.EqualTo("Picking..."), 
				"Status should not still be 'Picking...' after returning from background - this indicates MediaPicker async call is stuck");
		}
	}
}
#endif