#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8801 : _IssuesUITest
	{
		public Issue8801(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Attempt to read from field 'int android.view.ViewGroup$LayoutParams.width' on a null object reference";

		// Crash after navigation
		/*
		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		public void NotAddingElementsNativelyDoesntCrashAndroid()
		{
			App.WaitForElement("Success");
		}
		*/
	}
}
#endif