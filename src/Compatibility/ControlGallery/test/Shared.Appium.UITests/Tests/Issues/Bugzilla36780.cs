using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla36780 : IssuesUITest
	{
		const string TestImage = "TestImage";
		const string Gesture1Success = "Gesture1Success";
		const string Gesture2Success = "Gesture2Success";
		const string Gesture3Success = "Gesture3Success";

		public Bugzilla36780(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Multiple TapGestureRecognizers on an Object Are Not Fired";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void MultipleTapGestures()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(TestImage);
			RunningApp.Tap(TestImage);

			RunningApp.WaitForNoElement(Gesture1Success);
			RunningApp.WaitForNoElement(Gesture2Success);
			RunningApp.WaitForNoElement(Gesture3Success);
		}
	}
}