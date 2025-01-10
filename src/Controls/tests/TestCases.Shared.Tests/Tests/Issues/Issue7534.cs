#if TEST_FAILS_ON_ANDROID // Getting Java.Lang.IndexOutOfBoundsException on Android (https://github.com/dotnet/maui/issues/25836)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7534 : _IssuesUITest
	{
		public Issue7534(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Span with tail truncation and paragraph breaks with Java.Lang.IndexOutOfBoundsException";


		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		public void ExpectingPageNotToBreak()
		{
			App.WaitForElement("Success");
			//if it doesn't crash, we're good.
		}

	}
}
#endif
