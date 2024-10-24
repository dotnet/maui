#if ANDROID
using NUnit.Framework;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7534 : _IssuesUITest
	{
		public Issue7534(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Span with tail truncation and paragraph breaks with Java.Lang.IndexOutOfBoundsException";

		// Crash after navigation
		/*
		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void ExpectingPageNotToBreak()
		{
			App.Screenshot("Test passed, label is showing as it should!");
			//if it doesn't crash, we're good.
		}
		*/
	}
}
#endif