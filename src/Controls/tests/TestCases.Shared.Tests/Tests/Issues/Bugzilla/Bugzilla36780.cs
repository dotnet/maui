using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36780 : _IssuesUITest
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
			App.WaitForElement(TestImage);
			App.Tap(TestImage);

			App.WaitForElement(Gesture1Success);
			App.WaitForElement(Gesture2Success);
			App.WaitForElement(Gesture3Success);
		}
	}
}