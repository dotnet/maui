#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12213 : _IssuesUITest
	{
		public Issue12213(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Windows] TapGestureRecognizer not working on Entry";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Gestures)]
		public void TapGestureRecognizerNotWorkingOnEntry()
		{
			App.WaitForElement("Entry");
			App.Tap("Entry");
			VerifyScreenshot();
		}
	}
}
#endif