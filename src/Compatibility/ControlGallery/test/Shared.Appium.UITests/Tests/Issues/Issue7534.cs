#if ANDROID
using NUnit.Framework;
using UITest.Core;

namespace UITests
{
	public class Issue7534 : IssuesUITest
	{
		public Issue7534(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Span with tail truncation and paragraph breaks with Java.Lang.IndexOutOfBoundsException";

		// Crash after navigation
		/*
		[Test]
		[Category(UITestCategories.Label)]
		[FailsOnAndroid]
		public void ExpectingPageNotToBreak()
		{
			RunningApp.Screenshot("Test passed, label is showing as it should!");
			//if it doesn't crash, we're good.
		}
		*/
	}
}
#endif